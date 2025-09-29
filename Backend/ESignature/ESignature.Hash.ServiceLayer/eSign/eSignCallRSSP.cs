using System.Net;
using System.Text;
using Newtonsoft.Json;
using SdkTester.dataModel;

namespace SdkTester.eSign
{
    public class eSignCallRSSP
    {
        private static readonly HttpClientHandler handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        private static readonly HttpClient client = new HttpClient(handler);
        private const string USER_AGENT = "Mozilla/5.0";

        public string? URL;
        public string? relyingParty;
        public string? relyingPartyUser;
        public string? relyingPartyPassword;
        public string? relyingPartySignature;
        public string? relyingPartyKeyStore;
        public string? relyingPartyKeyStorePassword;

        public const string FUNCTION_GETCERTIFICATEDETAILFORSIGNCLOUD = "getCertificateDetailForSignCloud";
        public const string FUNCTION_PREPAREHASHSIGNINGFORSIGNCLOUD = "prepareHashSigningForSignCloud";
        public const string FUNCTION_GETSIGNATUREVALUEFORSIGNCLOUD = "getSignatureValueForSignCloud";

        public void SetParams()
        {
            URL = "https://rssp.fptdev.site/eSignCloud/restapi/";
            relyingParty = "GIC_DEMO";
            relyingPartyUser = "GIC_DEMO";
            relyingPartyPassword = "85pJrHNG";
            relyingPartySignature = "OKHIvMrcwijaoejjAP0dRHE2N3O3qcOBhLqBJ694T/ZHoOV197t9vaVg736oUPz1lKGFLzDk2I2qDPQSFw1JewgRj0QtU9nqRLMVwwdB4Tr06hxYxKptQCGHkq4K7pezgj0qWLoNAdmekJSFYETR1hogUFPJRJM8YmDfj7baQ690+S2BYU5PZk+i6bU0tEGGB8W3Oph5vFmWBUlUC680ntVmeE4TxYI+kN3pONlkiMY0/gUoTWe9nCtTt/dGLg2zyzyg0MDXkb5eUzcHaDordKSVAxdIV+9rwHqpmtBvVGOT3EMR8VjX8RErfcLMRx2psZP+8b6mI8EJFpOjQF26tw==";
            relyingPartyKeyStore = "files/GIC_DEMO.p12";
            relyingPartyKeyStorePassword = "W8jKYuJ4";
        }
      
        public SignCloudResp getCertificateDetailForSignCloud(string agreementUUID)
        {
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            string data2sign = relyingPartyUser +relyingPartyPassword + relyingPartySignature + timestamp;
            string pkcs1Signature = Utils.GetPKCS1Signature(data2sign, relyingPartyKeyStore, relyingPartyKeyStorePassword);

            var credential = new CredentialData
            {
                Username = relyingPartyUser,
                Password = relyingPartyPassword,
                Timestamp = timestamp,
                Signature = relyingPartySignature,
                Pkcs1Signature = pkcs1Signature
            };

            var req = new SignCloudReq
            {
                RelyingParty = relyingParty,
                AgreementUUID = agreementUUID,
                CredentialData = credential
            };

            string payload = JsonConvert.SerializeObject(req, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            string signature = Utils.GetSignature(payload, relyingPartyKeyStore, relyingPartyKeyStorePassword);

            string response = SendPost(FUNCTION_GETCERTIFICATEDETAILFORSIGNCLOUD, payload, signature);
            Console.WriteLine("Request Payload: " + payload);
            Console.WriteLine("Signature: " + signature);

            Console.WriteLine("Response: " + response);
          
            return JsonConvert.DeserializeObject<SignCloudResp>(response);
        }
        public SignCloudResp prepareHashSigningForSignCloud(string agreementUUID, int authorizeMethod, string authorizeCode, string mimeType, List<byte[]> hashes)
        {
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            string data2sign = relyingPartyUser + relyingPartyPassword + relyingPartySignature + timestamp;
            string pkcs1Signature = Utils.GetPKCS1Signature(data2sign, relyingPartyKeyStore, relyingPartyKeyStorePassword);

            var files = new List<MultipleSigningFileData>();
            foreach (var hash in hashes)
            {
                files.Add(new MultipleSigningFileData
                {
                    Hash = Utils.PrintHexBinary(hash),
                    MimeType = ESignCloudConstant.MIMETYPE_SHA256,
                    SigningFileName = "Hash_Name_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
            }

            var credential = new CredentialData
            {
                Username = relyingPartyUser,
                Password = relyingPartyPassword,
                Timestamp = timestamp,
                Signature = relyingPartySignature,
                Pkcs1Signature = pkcs1Signature
            };

            var req = new SignCloudReq
            {
                RelyingParty = relyingParty,
                AgreementUUID = agreementUUID,
                AuthorizeMethod = authorizeMethod,
                MessagingMode = ESignCloudConstant.SYNCHRONOUS,
                AuthorizeCode = authorizeCode,
                CertificateRequired = true,
                Language = "EN",
                MultipleSigningFileData = files,
                CredentialData = credential
            };

            string payload = JsonConvert.SerializeObject(req, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            string signature = Utils.GetSignature(payload, relyingPartyKeyStore, relyingPartyKeyStorePassword);
            string response = SendPost(FUNCTION_PREPAREHASHSIGNINGFORSIGNCLOUD, payload, signature);
            Console.WriteLine("Request Payload: " + payload);
            Console.WriteLine("Signature: " + signature);
            Console.WriteLine("Response: " + response);
            return JsonConvert.DeserializeObject<SignCloudResp>(response);
        }


        private string SendPost(string function, string payload, string signature)
        {
            var request = (HttpWebRequest)WebRequest.Create(URL + function);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("User-Agent", USER_AGENT);
            request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
            request.Headers.Add("signature", signature);
            request.Headers.Add("x-esignservice-name", "eSSC");

            using (var stream = request.GetRequestStream())
            {
                var bytes = Encoding.UTF8.GetBytes(payload);
                stream.Write(bytes, 0, bytes.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
