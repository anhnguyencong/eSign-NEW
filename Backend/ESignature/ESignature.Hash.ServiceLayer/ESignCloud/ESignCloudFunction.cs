using ESignature.HashServiceLayer.Settings;
using ESignature.HashServiceLayer.Settings;
using lib.rssp.exsig;
using lib.rssp.exsig.pdf;
using lib.rssp.sign;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ESignature.HashServiceLayer.ESignCloud
{
    public class ESignCloudFunction : SigningMethodSnyc
    {
        private readonly string AUTH_LOGIN = "auth/login";
        private readonly string CREDENTIALS_INFO = "credentials/info";
        private readonly string CREDENTIAL_AUTHORIZE = "credentials/authorize";
        private readonly string SIGNATURES_SIGNHASH = "signatures/signHash";
        private readonly string RelyingParty = "GIC";
        private readonly string Profile = "rssp-119.432-v2.0";
        private readonly string Language = "VN";
        private string RestUrl;
        private string RelyingPartyUser;
        private string RelyingPartyPassword;
        private string RelyingPartySignature;
        private string RelyingPartyKeyStore;
        private string RelyingPartyKeyStorePassword;
        private string AgreementUUID;
        private string CredentialID;
        private string PassCode;
        private string SAD;
        private string Bearer;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ESignCloudFunction> _logger;

        public ESignCloudFunction(IWebHostEnvironment webHostEnvironment, ILogger<ESignCloudFunction> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<List<byte[]>> SignPdf(RsspCloudSetting setting, List<FileDto> files, string description, DateTime? _approvalDate,
            string visiblePosition, string pageSign, Branch branchSetting = null)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            SetRsspCloudSetting(setting);
            stopwatch.Stop();
            _logger.LogWarning($"SetRsspCloudSetting: {stopwatch.ElapsedMilliseconds} ms");
         
            AuthLogin();
            stopwatch.Stop();
            _logger.LogWarning($"AuthLogin: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Reset();
            stopwatch.Start();
           
            var signPosition = !string.IsNullOrEmpty(visiblePosition);
            double boxWith = 140, boxHeght = 50;
            if (branchSetting.FullName.Split('\n').Length > 2)
                boxHeght += Math.Round((branchSetting.FullName.Split('\n').Length - 2) * 10.5);

            //if have description -> increase boxHeight
            if (!string.IsNullOrWhiteSpace(description))
            {
                description = description.Replace(';', '\n');
                boxHeght += description.Split('\n').Length * 10;

                double boxHeightIncrease = 0;

                foreach (var item in description.Split('\n'))
                {
                    if (item.Length > 32)
                    {
                        boxHeght += (item.Length / 32) * 10;
                        boxHeightIncrease = 5;
                    }
                }

                boxHeght += boxHeightIncrease;
            }
          

            string pPosition = "";
            double padding = 50;
            var src = new List<byte[]>();
            var filePasswords = new List<string>();
            var pPage = 1;
            foreach (var f in files)
            {
                if (signPosition)
                {
                    var pdfReader = new iTextSharp.text.pdf.PdfReader(f.FilePendingPath);
                    if (pageSign.ToUpper() == "LAST")
                    {
                        pPage = pdfReader.NumberOfPages;
                    }
                    else
                    {
                        if (!int.TryParse(pageSign, out pPage))
                        {
                            pPage = 1;
                        }
                    }
                    var psize = pdfReader.GetPageSize(pPage);
                    pPosition = GetPosition(psize.Width, psize.Height, boxWith, boxHeght, visiblePosition, padding);
                }

                byte[] data = await File.ReadAllBytesAsync(f.FilePendingPath);

                src.Add(data);
                filePasswords.Add(f.Password);
            }

            var iconSignatureUrl = string.Format("{0}/esigncloud/{1}", _webHostEnvironment.WebRootPath, "checkmarkicon.png");
            byte[] signature = File.ReadAllBytes(iconSignatureUrl);

            DateTime calendar = DateTime.Now;
            PdfProfile profile = new PdfProfile(PdfForm.B, Algorithm.SHA256);
            profile.SetBackground(DefaultColor.WHITE);

            //string signed = "Đã ký điện tử bởi:\n{signby}\n";
            string signed = "Đã ký điện tử bởi:\n";
            if (!string.IsNullOrEmpty(branchSetting.FullName))
            {
                //branchSetting SignerId + FullName được update và lưu trữ trong file: wwwroot/config/listbranch.json
                signed += branchSetting.FullName + "\n";             
            }
            else signed += "{signby}\n";

            if (!string.IsNullOrWhiteSpace(description))
            {
                signed = signed + description + "\n";
            }

            if (_approvalDate.HasValue)
            {
                profile.SetTextContent(signed + "Ký ngày: " + _approvalDate.Value.ToString("dd/MM/yyyy"));
            }
            else
            {
                profile.SetTextContent(signed + "Ký ngày: {date}");
            }

            profile.SetFont(DefaultFont.D_Times, 8, 1.5f, TextAlignment.ALIGN_CENTER, DefaultColor.BLACK);

            profile.AddLayer0Icon(signature, "15, 5, 30, 20");
            profile.SetSigningTime(calendar, "dd/MM/yyyy");
            profile.SetBorder(DefaultColor.BLACK);
            if (signPosition && !string.IsNullOrEmpty(pPosition))
            {
                profile.SetVisibleSignature(pPage.ToString(), pPosition);
            }
            else
            {
                profile.SetVisibleSignature("-30, -10", $"{boxWith}, {boxHeght}", "[[Chữ ký điện tử]]");
            }
            stopwatch.Stop();
            _logger.LogWarning($"Process file for Sign: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Reset();
            stopwatch.Start();
            var list = profile.Sign(this, src, new List<string>(filePasswords));
            stopwatch.Stop();
            _logger.LogWarning($"profile.Sign: {stopwatch.ElapsedMilliseconds} ms");

            return list;
        }

        private void SetRsspCloudSetting(RsspCloudSetting setting)
        {
            CredentialID = setting.CredentialID;
            PassCode = setting.PassCode;
            RestUrl = setting.RestUrl;
            RelyingPartyUser = setting.Username;
            RelyingPartyPassword = setting.Password;
            RelyingPartyKeyStore = Path.Combine(_webHostEnvironment.WebRootPath, "esigncloud", setting.KeyStore);
            RelyingPartyKeyStorePassword = setting.KeyStorePassword;
            AgreementUUID = setting.AgreementUUID;
            RelyingPartySignature = setting.Signature;
        }

        private RsspResponse CredentialsInfo()
        {
            RsspRequest request = new RsspRequest(this.RestUrl, this.RelyingPartyUser, this.RelyingPartyPassword,
                    this.RelyingPartySignature, this.RelyingPartyKeyStore, this.RelyingPartyKeyStorePassword,
                    this.Profile);
            request.lang = this.Language;
            request.agreementUUID = this.AgreementUUID;
            request.bearer = Bearer;
            // Khi ky phai truyen chain
            request.certificates = "chain";
            request.certInfo = false;
            request.authInfo = true;
            request.credentialID = this.CredentialID;

            var jsonReq = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var jsonResp = request.SendPost(RestUrl + CREDENTIALS_INFO, jsonReq, Function.DEFAULT);
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = new[] { new ByteArrayConverter() }
            };

            var signCloudResp = JsonConvert.DeserializeObject<RsspResponse>(jsonResp, jsonSerializerSettings);
            if (signCloudResp.error == 3005 || signCloudResp.error == 3006)
            {
                Bearer = null;
                AuthLogin();
                return CredentialsInfo();
            }
            return signCloudResp;
        }

        private void AuthLogin()
        {
            if (Bearer != null)
            {
                return;
            }

            var request = new RsspRequest(this.RestUrl, this.RelyingPartyUser, this.RelyingPartyPassword,
                this.RelyingPartySignature, this.RelyingPartyKeyStore, this.RelyingPartyKeyStorePassword, this.Profile)
            {
                rememberMe = false,
                relyingParty = this.RelyingParty,
                lang = this.Language
            };

            var jsonReq = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var jsonResp = request.SendPost(RestUrl + AUTH_LOGIN, jsonReq, Function.AUTH_LOGIN_SSL_ONLY);
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = new[] { new ByteArrayConverter() }
            };
            var signCloudResp = JsonConvert.DeserializeObject<RsspResponse>(jsonResp, jsonSerializerSettings);
            Bearer = signCloudResp.accessToken;
        }

        private RsspResponse CredentialSignHash(List<string> hash)
        {
            var request = new RsspRequest(this.RestUrl, this.RelyingPartyUser, this.RelyingPartyPassword,
                    this.RelyingPartySignature, this.RelyingPartyKeyStore, this.RelyingPartyKeyStorePassword,
                    this.Profile)
            {
                lang = this.Language,
                agreementUUID = this.AgreementUUID,
                bearer = Bearer,
                credentialID = this.CredentialID,
                SAD = this.SAD,
                documentDigests = new DocumentDigests()
            };
            request.documentDigests.hashAlgorithmOID = "2.16.840.1.101.3.4.2.1";
            request.documentDigests.hashes = hash;
            request.signAlgo = "1.2.840.113549.1.1.1";

            var jsonReq = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var jsonResp = request.SendPost(RestUrl + SIGNATURES_SIGNHASH, jsonReq, Function.DEFAULT);
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = new[] { new ByteArrayConverter() }
            };

            var signCloudResp = JsonConvert.DeserializeObject<RsspResponse>(jsonResp, jsonSerializerSettings);
            if (signCloudResp.error == 3005 || signCloudResp.error == 3006)
            {
                Bearer = null;
                AuthLogin();
                return CredentialSignHash(hash);
            }
            return signCloudResp;
        }

        private void CredentialsAuthorize(List<string> hash, byte[] bytes)
        {
            var request = new RsspRequest(this.RestUrl, this.RelyingPartyUser, this.RelyingPartyPassword, this.RelyingPartySignature,
                this.RelyingPartyKeyStore, this.RelyingPartyKeyStorePassword, this.Profile)
            {
                lang = this.Language,
                agreementUUID = this.AgreementUUID,
                bearer = Bearer,
                credentialID = this.CredentialID,
                numSignatures = hash.Count,
                documentDigests = new DocumentDigests()
            };
            request.documentDigests.hashAlgorithmOID = "2.16.840.1.101.3.4.2.1";
            request.authorizeCode = this.PassCode;
            var jsonReq = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var jsonResp = request.SendPost(RestUrl + CREDENTIAL_AUTHORIZE, jsonReq, Function.DEFAULT);
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = new[] { new ByteArrayConverter() }
            };
            var signCloudResp = JsonConvert.DeserializeObject<RsspResponse>(jsonResp, jsonSerializerSettings);

            this.SAD = signCloudResp.SAD;

            if (signCloudResp.error == 3005 || signCloudResp.error == 3006)
            {
                Bearer = null;
                AuthLogin();
                CredentialsAuthorize(hash, bytes);
            }
        }

        List<string> SigningMethodSnyc.Sign(List<string> hashList)
        {
            CredentialsAuthorize(hashList, null);
            RsspResponse response = CredentialSignHash(hashList);
            List<string> sigList = new List<string>(response.signatures);
            return sigList;
        }

        List<string> SigningMethodSnyc.GetCert()
        {
            RsspResponse response = CredentialsInfo();
            List<string> certList = new List<string>();
            foreach (string base64Cert in response.cert.certificates)
            {
                certList.Add(base64Cert);
            }
            return certList;
        }

        private string GetPosition(float pageWith, float pageHeight, double boxWith, double boxHeght, string position, double padding)
        {
            double x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            switch (position.ToUpper())
            {
                case "TOPLEFT":
                    x1 = 0 + padding;
                    y1 = pageHeight - boxHeght - padding;
                    break;
                case "TOPRIGHT":
                    x1 = Math.Round(pageWith - boxWith) - padding;
                    y1 = pageHeight - boxHeght - padding;
                    break;
                case "BOTTOMLEFT":
                    x1 = 0 + padding;
                    y1 = 0 + padding;
                    break;
                case "BOTTOMRIGHT":
                    x1 = Math.Round(pageWith - boxWith) - padding;
                    y1 = 0 + padding;
                    break;
                default:
                    var arryPosition = position.Split(',');
                    if (arryPosition.Length >= 2)
                    {
                        double.TryParse(arryPosition[0], out x1);
                        double.TryParse(arryPosition[1], out y1);
                    }
                    break;
            }
            x2 = x1 + boxWith;
            y2 = y1 + boxHeght;
            return $"{Math.Round(x1)}, {Math.Round(y1)}, {Math.Round(x2)}, {Math.Round(y2)}";
        }
    }
}