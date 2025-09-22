using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ESignature.ServiceLayer.ESignCloud
{
    public class FileDto
    {
        public string FilePendingPath { get; set; }
        public string Password { get; set; }
    }

    public class RsspRequest
    {
        public RsspRequest(string URL, string relyingPartyUser, string relyingPartyPassword, string relyingPartySignature, string relyingPartyKeyStore, string relyingPartyKeyStorePassword, string profile)
        {
            this.URL = URL;
            this.relyingPartyUser = relyingPartyUser;
            this.relyingPartyPassword = relyingPartyPassword;
            this.relyingPartySignature = relyingPartySignature;
            this.relyingPartyKeyStore = relyingPartyKeyStore;
            this.relyingPartyKeyStorePassword = relyingPartyKeyStorePassword;
            this.profile = profile;
        }

        private string URL;
        private string relyingPartyUser;
        private string relyingPartyPassword;
        private string relyingPartySignature;
        private string relyingPartyKeyStore;
        private string relyingPartyKeyStorePassword;

        public string relyingParty { get; set; }
        public string agreementUUID { get; set; }
        public string authorizeCode { get; set; }
        public string oldPassphrase { get; set; }
        public string newPhone { get; set; }
        public string otpOldPhone { get; set; }
        public string otpNewPhone { get; set; }
        public string otpOldEmail { get; set; }
        public string otpNewEmail { get; set; }
        public string user { get; set; }
        public string oldPassword { get; set; }
        public string ownerUUID { get; set; }
        public string signatureAlgorithmParams { get; set; }
        public string createdRP { get; set; }
        public string newPassphrase { get; set; }
        public string userType { get; set; }
        public string relyingPartyBillCode { get; set; }
        public bool? rememberMe { get; set; }
        public string lang { get; set; }
        public int? tokenType { get; set; }
        public string token { get; set; }
        public string credentialID { get; set; }
        public string certificates { get; set; }
        public bool? certInfo { get; set; }
        public bool? authInfo { get; set; }
        public string notificationTemplate { get; set; }
        public string notificationSubject { get; set; }
        public string billCode { get; set; }
        public int? numSignatures { get; set; }
        public string notificationTitle { get; set; }
        public string notificationMessage { get; set; }

        //public ClientInfo clientInfo{ get; set; }
        public string message { get; set; }

        public string logoURI { get; set; }
        public string bgImageURI { get; set; }
        public string rpIconURI { get; set; }
        public string rpName { get; set; }
        public string confirmationPolicy { get; set; }
        public int? expirationDuration { get; set; }
        public bool? vcEnabled { get; set; }
        public bool? acEnabled { get; set; }
        public int? messagingMode { get; set; }
        public string SAD { get; set; }
        public SearchConditions SearchConditions { get; set; }
        public int? ownerID { get; set; }

        [JsonIgnore]
        public string bearer { get; set; }

        public string newPassword { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string requestType { get; set; }
        public string newEmail { get; set; }
        public string profile { get; set; }

        public string messageCaption { get; set; }
        public string operationMode { get; set; }
        public string scaIdentity { get; set; }
        public string responseURI { get; set; }
        public int? validityPeriod { get; set; }
        public string[] documents { get; set; }
        public string signAlgo { get; set; }
        public string signAlgoParams { get; set; }
        public string signatureFormat { get; set; }
        public string conformanceLevel { get; set; }
        public string signedEnvelopeProperty { get; set; }
        public DocumentDigests documentDigests { get; set; }
        public string requestID { get; set; }
        public string caName { get; set; }
        public string certificateProfile { get; set; }
        public string authMode { get; set; }
        public CertificateDetails certDetails { get; set; }
        public string certificate { get; set; }
        public string fullname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string identificationType { get; set; }
        public string identification { get; set; }
        public string twoFactorMethod { get; set; }
        public bool? registerTSEEnabled { get; set; }
        public string loa { get; set; }
        public string kycEvidence { get; set; }
        public SearchConditions SearchCondition { get; set; }
        public int? pageNumber { get; set; }
        public int? recordCount { get; set; }

        public string SendPost(string url, string payload, Function function)
        {
            string result;
            string endpointUrl = url;

            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 9999;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpointUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            // httpWebRequest.Headers["Authorization"] = getAuthorization();
            string authorization = "";
            switch (function)
            {
                case Function.INFO:
                    break;

                case Function.AUTH_LOGIN:
                    if (username == null)
                    {
                        throw new Exception("Username can't be null");
                    }
                    if (password == null)
                    {
                        throw new Exception("Password can't be null");
                    }
                    authorization = GetAuthorization(username, password);
                    username = null;
                    userType = null;
                    break;

                case Function.AUTH_LOGIN_SSL_ONLY:
                    authorization = GetAuthorization();
                    break;

                default:
                    if (bearer != null)
                    {
                        authorization = "Bearer " + bearer;
                    }
                    else
                    {
                        throw new Exception("Bearer can't be null");
                    }
                    break;
            }
            httpWebRequest.Headers["Authorization"] = authorization;

            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(payload);
            }

            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }

        private string GetAuthorization()
        {
            string timestamp = Utils.CurrentTimeMillis().ToString();
            string data2sign = relyingPartyUser + relyingPartyPassword + relyingPartySignature + timestamp;
            string pkcs1Signature = Utils.GetPKCS1Signature(data2sign, relyingPartyKeyStore, relyingPartyKeyStorePassword);
            return "SSL2 " + Utils.Base64Encode((relyingPartyUser + ":" + relyingPartyPassword + ":" + relyingPartySignature + ":" + timestamp + ":" + pkcs1Signature));
        }

        private string GetAuthorization(string username, string password)
        {
            string timestamp = Utils.CurrentTimeMillis().ToString();
            string data2sign = relyingPartyUser + relyingPartyPassword + relyingPartySignature + timestamp;
            string pkcs1Signature = Utils.GetPKCS1Signature(data2sign, relyingPartyKeyStore, relyingPartyKeyStorePassword);

            return "SSL2 " + Utils.Base64Encode(
                    relyingPartyUser + ":" + relyingPartyPassword + ":" +
                    relyingPartySignature + ":" + timestamp + ":" + pkcs1Signature)
                    + ", basic " + Utils.Base64Encode(userType + ":" + username + ":" + password);
        }
    }

    public class CertificateDetails
    {
        public string commonName { get; set; }
        public string organization { get; set; }
        public string organizationUnit { get; set; }
        public string title { get; set; }
        public string email { get; set; }
        public string telephoneNumber { get; set; }
        public string location { get; set; }
        public string stateOrProvince { get; set; }
        public string country { get; set; }
        public Identification[] identifications { get; set; }
    }

    public class SearchConditions
    {
        public string certificateStatus { get; set; }
        public string cetificatePurpose { get; set; }
        public string taxID { get; set; }
        public string budgetID { get; set; }
        public string personalID { get; set; }
        public string passportID { get; set; }
        public Identification identification { get; set; }
        public string[] agreementStates { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
    }

    public class Identification
    {
        public string type { get; set; }
        public string value { get; set; }

        public Identification(string type, string value)
        {
            this.type = type;
            this.value = value;
        }
    }

    public class DocumentDigests
    {
        public List<string> hashes { get; set; }
        public string hashAlgorithmOID { get; set; }
    }

    public class RsspResponse
    {
        public string authorizationPhone { get; set; }
        public string authorizationEmail { get; set; }
        public int temporaryLockTime { get; set; }
        public string version { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string logo { get; set; }
        public string[] languages { get; set; }
        public string[] authTypes { get; set; }
        public string[] methods { get; set; }
        public int error { get; set; }
        public string errorDescription { get; set; }
        public string billCode { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public int expiresIn { get; set; }
        public string username { get; set; }
        public Cert cert { get; set; }
        public int[] authorizeMethod { get; set; }
        public int numSignatures { get; set; }
        public string authMode { get; set; }
        public int SCAL { get; set; }
        public string language { get; set; }
        public string SAD { get; set; }
        public int remainingCounter { get; set; }
        public string[] signatures { get; set; }
        public string sharedMode { get; set; }
        public string createdRP { get; set; }
        public string[] authModeSupported { get; set; }
        public string contractExpirationDt { get; set; }
        public int remainingSigningCounter { get; set; }
        public string[] signaturePolicies { get; set; }
        public string[] servicePolicies { get; set; }
        public string[] operationModes { get; set; }
        public string rpRequestID { get; set; }
        public string requestID { get; set; }
        public bool rememberMe { get; set; }
        public int tempLockoutDuration { get; set; }
        public string authorizeToken { get; set; }
        public string twoFactorMethod { get; set; }
        public int tempLockDuration { get; set; }
        public int multisign { get; set; }
        public string[] authModes { get; set; }
        public string contractExpirationDate { get; set; }
        public bool defaultPassphrase { get; set; }
        public string[] documentWithSignature { get; set; }
        public string[] signatureObject { get; set; }
        public bool productionEnabled { get; set; }
        public string responseID { get; set; }
        public string[] certificates { get; set; }
        public string csr { get; set; }
        public string credentialID { get; set; }
        public string state { get; set; }
        public string fullname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string identificationType { get; set; }
        public string identification { get; set; }
        public string ownerUUID { get; set; }
        public string loa { get; set; }
        public string kycEvidence { get; set; }
        public int currentPage { get; set; }
        public int recordCount { get; set; }
        public int recordTotal { get; set; }
        public string claims { get; set; }
        public int responseCode { get; set; }
        public string responseMessage { get; set; }
    }

    public class Cert
    {
        public string status { get; set; }
        public string[] certificates { get; set; }
        public string credentialID { get; set; }
        public string issuerDN { get; set; }
        public string serialNumber { get; set; }
        public string thumbprint { get; set; }
        public string subjectDN { get; set; }
        public string validFrom { get; set; }
        public string validTo { get; set; }
        public string purpose { get; set; }
        public string multisign { get; set; }
        public int numSignatures { get; set; }
        public int remainingSigningCounter { get; set; }
        public int version { get; set; }
        public string authorizationEmail { get; set; }
        public string authorizationPhone { get; set; }
        public CertificateProfile certificateProfile { get; set; }
    }

    public class CertificateProfile
    {
        public string name { get; set; }
        public string description { get; set; }
    }

    public enum Function
    {
        INFO,
        AUTH_LOGIN,
        AUTH_LOGIN_SSL_ONLY,
        DEFAULT
    }
}