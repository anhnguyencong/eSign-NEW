using Newtonsoft.Json;

namespace SdkTester.dataModel
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SignCloudResp
    {
        [JsonProperty("responseCode")]
        public int ResponseCode { get; set; }

        [JsonProperty("responseMessage")]
        public string ResponseMessage { get; set; }

        [JsonProperty("billCode")]
        public string BillCode { get; set; }

        [JsonProperty("timestamp")]
        [JsonConverter(typeof(EpochDateTimeConverter))]

        public DateTime? Timestamp { get; set; }

        [JsonProperty("logInstance")]
        public int LogInstance { get; set; }

        [JsonProperty("notificationMessage")]
        public string NotificationMessage { get; set; }

        [JsonProperty("remainingCounter")]
        public int RemainingCounter { get; set; }

        [JsonProperty("signedFileData")]
        public byte[] SignedFileData { get; set; }

        [JsonProperty("signedFileName")]
        public string SignedFileName { get; set; }

        [JsonProperty("authorizeCredential")]
        public string AuthorizeCredential { get; set; }

        [JsonProperty("signedFileUUID")]
        public string SignedFileUUID { get; set; }

        [JsonProperty("mimeType")]
        public string MimeType { get; set; }

        [JsonProperty("certificateDN")]
        public string CertificateDN { get; set; }

        [JsonProperty("certificateSerialNumber")]
        public string CertificateSerialNumber { get; set; }

        [JsonProperty("certificateThumbprint")]
        public string CertificateThumbprint { get; set; }

        [JsonProperty("validFrom")]
        [JsonConverter(typeof(EpochDateTimeConverter))]

        public DateTime? ValidFrom { get; set; }

        [JsonProperty("validTo")]
        [JsonConverter(typeof(EpochDateTimeConverter))]

        public DateTime? ValidTo { get; set; }


        [JsonProperty("issuerDN")]
        public string IssuerDN { get; set; }

        [JsonProperty("uploadedFileUUID")]
        public string UploadedFileUUID { get; set; }

        [JsonProperty("downloadedFileUUID")]
        public string DownloadedFileUUID { get; set; }

        [JsonProperty("downloadedFileData")]
        public byte[] DownloadedFileData { get; set; }

        [JsonProperty("signatureValue")]
        public string SignatureValue { get; set; }

        [JsonProperty("authorizeMethod")]
        public int AuthorizeMethod { get; set; }

        [JsonProperty("notificationSubject")]
        public string NotificationSubject { get; set; }

        [JsonProperty("dmsMetaData")]
        public string DmsMetaData { get; set; }

        [JsonProperty("csr")]
        public string Csr { get; set; }

        [JsonProperty("certificate")]
        public string Certificate { get; set; }

        [JsonProperty("certificateStateID")]
        public int CertificateStateID { get; set; }

        [JsonProperty("multipleSignedFileData")]
        public List<MultipleSignedFileData> MultipleSignedFileData { get; set; }

        [JsonProperty("sicUrl")]
        public string SicUrl { get; set; }

        [JsonProperty("subjectID")]
        public string SubjectID { get; set; }

        // Optional: Constructors
        public SignCloudResp() { }

        public SignCloudResp(int responseCode, string responseMessage)
        {
            ResponseCode = responseCode;
            ResponseMessage = responseMessage;
        }
    }
}
