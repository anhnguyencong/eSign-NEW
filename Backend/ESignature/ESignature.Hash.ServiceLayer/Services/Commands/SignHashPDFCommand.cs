using Aspose.Words.Shaping;
using AutoMapper;
using ESignature.HashServiceLayer.Settings;
using MediatR;
using Microsoft.Extensions.Logging;
using RSSP.AgentSdk.CSharp.Data;
using RSSP.AgentSdk.CSharp.eSign;
using SdkTester.dataModel;
using SdkTester.eSign;
using System.Reflection.PortableExecutable;
using System.Text;


namespace ESignature.HashServiceLayer.Services.Commands
{
    public class SignHashPDFCommand : IRequest<bool>
    {
        public string SignerId { get; set; } //dùng để tạo folder lưu file cert của signCloudResp.Certificate;
        public HashRsspCloudSetting HashRsspCloudSetting { get; set; }

        // danh sách fullname branch kí để kiểm soát việc xuống dòng của tên khi kí
        public Branch BranchSetting { get; set; }
        public string FilePath { get; set; }
        public string FilePassword { get; set; }
        public string CompletedFileName { get; set; }
        public string CompletedFilePath { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Description { get; set; }
        public string PageSign { get; set; }
        public string VisiblePosition { get; set; }
    }

    public class SignHashPDFCommandHandler : IRequestHandler<SignHashPDFCommand, bool>
    {
        private readonly ILogger<SignHashPDFCommandHandler> _logger;

        public SignHashPDFCommandHandler(ILogger<SignHashPDFCommandHandler> logger)
        {
            _logger = logger;
        }
        public async Task<bool> Handle(SignHashPDFCommand request, CancellationToken cancellationToken)
        {
            var result = await ProcessPdf(request, cancellationToken);
            return true;
        }

        private async Task<bool> ProcessPdf(SignHashPDFCommand request, CancellationToken cancellationToken)
        {
            try
            {
                eSignCallRSSP service = new eSignCallRSSP();
                //ESignCloudClient eSignCloudClient = new ESignCloudClient("9090");
                ESignCloudClient eSignCloudClient = new ESignCloudClient(request.HashRsspCloudSetting.ESignCloudClientPort);
                string projectRoot = AppContext.BaseDirectory;                

                request.HashRsspCloudSetting.KeyStore = Path.Combine(projectRoot, request.HashRsspCloudSetting.KeyStore);

                service.SetParams(request.HashRsspCloudSetting);

                SignCloudResp signCloudResp = null;
                List<byte[]> hashes = new List<byte[]>();
                List<byte[]> signatures = new List<byte[]>();
                List<byte[]> certFiles = new List<byte[]>();
                List<byte[]> pdfFiles = new List<byte[]>();
                List<SignerProperties> signerList = new List<SignerProperties>();
                SignerProperties signer1 = new SignerProperties();


                signCloudResp = service.getCertificateDetailForSignCloud(request.HashRsspCloudSetting.AgreementUUID);
                var cert = signCloudResp.Certificate;

                _logger.LogDebug($"Certificate of {request.SignerId}:{signCloudResp.Certificate}");

                //*********Begin Page Number**************************************************//
                var pPage = 1;
                //signer1.PageNo = "1";
                if (request.PageSign.ToUpper() == "LAST")
                {
                    var pdfReader = new iTextSharp.text.pdf.PdfReader(request.FilePath);
                    pPage = pdfReader.NumberOfPages;
                }
                else
                {
                    if (!int.TryParse(request.PageSign, out pPage))
                    {
                        pPage = 1;
                    }
                }
                //*********End Page Number**************************************************//

                //*********Begin Signed Rectangle**************************************************//
                //signer1.Rectangle = "400, 80, 550, 140";
                signer1.Rectangle = request.VisiblePosition;
                //*********End Signed Rectangle**************************************************//
                signer1.IsOffset = false;
                signer1.FontSize = 8.0f;
                signer1.Reason = "";
                signer1.Location = "";
                signer1.VisibleValidationSymbol = false;
                signer1.DateFormat = "dd/MM/yyyy";

                //*********Begin Tên chữ kí**************************************************//
                //signer1.SignerInformation = "Đã ký điện tử bởi:\n {signby}\nKý ngày: {date}";
                var sb = new StringBuilder();


                sb.Append("Đã ký điện tử bởi:\n");
                if (!string.IsNullOrEmpty(request.BranchSetting.FullName))
                {
                    //branchSetting SignerId + FullName được update và lưu trữ trong file: wwwroot/config/listbranch.json
                    sb.Append(request.BranchSetting.FullName).Append("\n");
                }
                else sb.Append("{signby}\n");

                if (!string.IsNullOrWhiteSpace(request.Description))
                {
                    sb.Append(request.Description).Append("\n");
                }
                if (request.ApprovalDate.HasValue)
                {
                    sb.Append("Ký ngày: ").Append(request.ApprovalDate.Value.ToString("dd/MM/yyyy"));
                }
                else
                {
                    sb.Append("Ký ngày: {date}");
                }
                signer1.SignerInformation = sb.ToString();

                //*********End Tên chữ kí**************************************************//


                signer1.TextAligment = TextAlignment.CENTER_TOP;
                signer1.TextPaddingLeft = 0;
                signer1.TextPaddingRight = 0;
                signer1.IsPlaceAll = false;
                signer1.ImageAligment = ImageAligment.LEFT_BOTTOM;
                string imagePath = Path.Combine(projectRoot, "wwwroot", "imageCheck.png");
                signer1.ImageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

                signerList.Clear();
                signerList.Add(signer1);

                pdfFiles.Clear();

                pdfFiles.Add(File.ReadAllBytes(request.FilePath));

                certFiles.Clear();
                if (!string.IsNullOrEmpty(cert))
                {
                    if (!Directory.Exists(Path.Combine(projectRoot, "files/certs", request.SignerId)))
                    {
                        Directory.CreateDirectory(Path.Combine(projectRoot, "files/certs", request.SignerId));
                    }

                    string tempCertPath = Path.Combine(projectRoot, "files/certs", request.SignerId, "cert_from_api.pem");
                    File.WriteAllText(tempCertPath, cert);
                    certFiles.Add(File.ReadAllBytes(tempCertPath));
                    certFiles.Add(File.ReadAllBytes(Path.Combine(projectRoot, "files/certs/cert_ca.pem")));
                    certFiles.Add(File.ReadAllBytes(Path.Combine(projectRoot, "files/certs/cert_root.pem")));
                }
                else
                {
                    _logger.LogError($"Không lấy được cert từ RSSP Cloud cho signerId: {request.SignerId}");
                }

                var hashResp = eSignCloudClient.getHashPDF(pdfFiles, certFiles, signerList);


                _logger.LogDebug($"getHashPDF:Response Code of {request.SignerId}:{hashResp.ResponseCode}");

                _logger.LogDebug($"getHashPDF:Message of {request.SignerId}:{hashResp.Message}");

                hashes.Clear();
                foreach (var result in hashResp.Results)
                {
                    hashes.Add(result.Hash);
                }


                signCloudResp = service.prepareHashSigningForSignCloud(request.HashRsspCloudSetting.AgreementUUID,
                    ESignCloudConstant.AUTHORISATION_METHOD_PASSCODE, request.HashRsspCloudSetting.PassCode,
                    ESignCloudConstant.MIMETYPE_SHA256, hashes);

                signatures.Clear();

                _logger.LogDebug($"prepareHashSigningForSignCloud:Response Code of {request.SignerId}:{signCloudResp.ResponseCode}");
                _logger.LogDebug($"prepareHashSigningForSignCloud:Response Message of {request.SignerId}:{signCloudResp.ResponseMessage}");

                if (hashes.Count > 1)
                {
                    foreach (var item in signCloudResp.MultipleSignedFileData)
                    {
                        signatures.Add(Convert.FromBase64String(item.SignatureValue));
                    }
                }
                else
                {
                    signatures.Add(Convert.FromBase64String(signCloudResp.SignatureValue));
                }
                _logger.LogDebug($"Đã lấy {signatures.Count} chữ ký.");

                var signedResp = eSignCloudClient.appendSignaturePDF(certFiles, hashes, signatures);

                _logger.LogDebug($"appendSignaturePDF:Response Code of {request.SignerId}:{signedResp.ResponseCode}");
                _logger.LogDebug($"appendSignaturePDF:Response Message of {request.SignerId}:{signedResp.Message}");

                for (int i = 0; i < signedResp.ListSigned.Count; i++)
                {
                    string outputPath = request.CompletedFilePath;// $"{projectRoot}/files/finalTest/test_{i + 1}.signed.pdf";
                    var completePath = Path.GetDirectoryName(outputPath);
                    if (!Directory.Exists(completePath))
                        Directory.CreateDirectory(completePath);
                    File.WriteAllBytes(outputPath, signedResp.ListSigned[i]);

                    _logger.LogDebug($"Đã lưu: {outputPath} ({signedResp.ListSigned[i].Length} bytes)");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Lỗi khi ký file cho signerId: {request.SignerId}. Chi tiết: {ex}");
                return false;
            }

            return true;
        }
    }
}
