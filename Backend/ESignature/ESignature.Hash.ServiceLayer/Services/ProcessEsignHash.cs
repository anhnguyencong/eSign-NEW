using RSSP.AgentSdk.CSharp.Data;
using RSSP.AgentSdk.CSharp.eSign;
using SdkTester.dataModel;
using SdkTester.eSign;
using System.Diagnostics;
using System.Text;

namespace ESignature.HashServiceLayer.Services
{
    
    public class ProcessEsignHash
    {
        public static string agreementUUID = "55ACEDBF-2AF9-4917-805E-87359394F763";
        public static string passCode = "12345678";
        public static string cert = "";
        public ProcessEsignHash()
        {

        }
        public void ProcessPdf()
        {
            Console.OutputEncoding = Encoding.UTF8;
            eSignCallRSSP service = new eSignCallRSSP();
            ESignCloudClient eSignCloudClient = new ESignCloudClient("9090");
            service.SetParams();

            SignCloudResp signCloudResp = null;
            List<byte[]> hashes = new List<byte[]>();
            List<byte[]> signatures = new List<byte[]>();
            List<byte[]> certFiles = new List<byte[]>();
            List<byte[]> pdfFiles = new List<byte[]>();
            List<SignerProperties> signerList = new List<SignerProperties>();
            SignerProperties signer1 = new SignerProperties();
            bool running = true;


            Stopwatch sw = new Stopwatch();
            sw.Start();
            //var input = Console.ReadLine();
            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

            //******************************
            //1
            Console.WriteLine("\nLấy chứng thư số");
            Console.WriteLine("------------------------------------");
            signCloudResp = service.getCertificateDetailForSignCloud(agreementUUID);
            cert = signCloudResp.Certificate;

            Console.WriteLine("Certificate: " + signCloudResp.Certificate);
            //2
            Console.WriteLine("\nTạo hash file PDF");
            Console.WriteLine("------------------------------------");
            signer1.PageNo = "1";
            signer1.Rectangle = "400, 80, 550, 140";
            signer1.IsOffset = false;
            signer1.FontSize = 8.0f;
            signer1.Reason = "";
            signer1.Location = "";
            signer1.VisibleValidationSymbol = false;
            signer1.DateFormat = "dd/MM/yyyy";
            signer1.SignerInformation = "Đã ký điện tử bởi:\n {signby}\nKý ngày: {date}";
            signer1.TextAligment = TextAlignment.CENTER_TOP;
            signer1.TextPaddingLeft = 0;
            signer1.TextPaddingRight = 0;
            signer1.IsPlaceAll = false;
            signer1.ImageAligment = ImageAligment.LEFT_BOTTOM;
            string imagePath = Path.Combine(projectRoot, "files", "imageCheck.png");
            signer1.ImageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

            signerList.Clear();
            signerList.Add(signer1);

            pdfFiles.Clear();
            pdfFiles.Add(File.ReadAllBytes(Path.Combine(projectRoot, "files", "test.pdf")));

            certFiles.Clear();
            if (!string.IsNullOrEmpty(cert))
            {
                string tempCertPath = Path.Combine(projectRoot, "files/certs/temp_cert_from_api.pem");
                File.WriteAllText(tempCertPath, cert);
                certFiles.Add(File.ReadAllBytes(tempCertPath));
                certFiles.Add(File.ReadAllBytes(Path.Combine(projectRoot, "files/certs/cert_ca.pem")));
                certFiles.Add(File.ReadAllBytes(Path.Combine(projectRoot, "files/certs/cert_root.pem")));
            }
            else
            {
                Console.WriteLine("Chưa có chứng thư số từ case '1'. Vui lòng thực hiện case '1' trước.");
                return;
            }

            var hashResp = eSignCloudClient.getHashPDF(pdfFiles, certFiles, signerList);
            Console.WriteLine("Response Code: " + hashResp.ResponseCode);
            Console.WriteLine("Message: " + hashResp.Message);
            hashes.Clear();
            foreach (var result in hashResp.Results)
            {
                Console.WriteLine("    hash: " + Convert.ToBase64String(result.Hash));
                hashes.Add(result.Hash);
            }

            //3
            Console.WriteLine("\nKý hash...");
            Console.WriteLine("------------------------------------");
            signCloudResp = service.prepareHashSigningForSignCloud(agreementUUID,
                ESignCloudConstant.AUTHORISATION_METHOD_PASSCODE, passCode,
                ESignCloudConstant.MIMETYPE_SHA256, hashes);

            signatures.Clear();
            Console.WriteLine("Response Code: " + signCloudResp.ResponseCode);
            Console.WriteLine("Message: " + signCloudResp.ResponseMessage);
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
            Console.WriteLine("Đã lấy " + signatures.Count + " chữ ký.");
            //4
            Console.WriteLine("\nĐóng gói Signature");
            Console.WriteLine("------------------------------------");
            var signedResp = eSignCloudClient.appendSignaturePDF(certFiles, hashes, signatures);
            Console.WriteLine("Response Code: " + signedResp.ResponseCode);
            Console.WriteLine("Message: " + signedResp.Message);

            for (int i = 0; i < signedResp.ListSigned.Count; i++)
            {
                string outputPath = $"{projectRoot}/files/finalTest/test_{i + 1}.signed.pdf";
                File.WriteAllBytes(outputPath, signedResp.ListSigned[i]);

                Console.WriteLine($"Đã lưu: {outputPath} ({signedResp.ListSigned[i].Length} bytes)");
            }
            sw.Stop();
            Console.WriteLine($"Total Time: {sw.ElapsedMilliseconds} ms.");
            //******************************
        }
    }
}
