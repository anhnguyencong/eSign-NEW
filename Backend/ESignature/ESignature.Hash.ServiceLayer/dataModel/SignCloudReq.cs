using Newtonsoft.Json;

namespace SdkTester.dataModel
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SignCloudReq
    {
        [JsonProperty("relyingParty")]
        public string RelyingParty { get; set; }

        [JsonProperty("relyingPartyBillCode")]
        public string RelyingPartyBillCode { get; set; }

        [JsonProperty("agreementUUID")]
        public string AgreementUUID { get; set; }

        [JsonProperty("sharedAgreementUUID")]
        public string SharedAgreementUUID { get; set; }

        [JsonProperty("sharedRelyingParty")]
        public string SharedRelyingParty { get; set; }

        [JsonProperty("mobileNo")]
        public string MobileNo { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("certificateProfile")]
        public string CertificateProfile { get; set; }

        [JsonProperty("agreementDetails")]
        public AgreementDetails AgreementDetails { get; set; }

        [JsonProperty("credentialData")]
        public CredentialData CredentialData { get; set; }

        [JsonProperty("signingFileUUID")]
        public string SigningFileUUID { get; set; }

        [JsonProperty("signingFileData")]
        public byte[] SigningFileData { get; set; }

        [JsonProperty("signingFileName")]
        public string SigningFileName { get; set; }

        [JsonProperty("mimeType")]
        public string MimeType { get; set; }

        [JsonProperty("notificationTemplate")]
        public string NotificationTemplate { get; set; }

        [JsonProperty("notificationSubject")]
        public string NotificationSubject { get; set; }

        [JsonProperty("timestampEnabled")]
        public bool TimestampEnabled { get; set; }

        [JsonProperty("ltvEnabled")]
        public bool LtvEnabled { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("authorizeCode")]
        public string AuthorizeCode { get; set; }

        [JsonProperty("postbackEnabled")]
        public bool PostbackEnabled { get; set; }

        [JsonProperty("noPadding")]
        public bool NoPadding { get; set; }

        [JsonProperty("authorizeMethod")]
        public int AuthorizeMethod { get; set; }

        [JsonProperty("uploadingFileData")]
        public byte[] UploadingFileData { get; set; }

        [JsonProperty("downloadingFileUUID")]
        public string DownloadingFileUUID { get; set; }

        [JsonProperty("currentPasscode")]
        public string CurrentPasscode { get; set; }

        [JsonProperty("newPasscode")]
        public string NewPasscode { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("hashAlgorithm")]
        public string HashAlgorithm { get; set; }

        [JsonProperty("encryption")]
        public string Encryption { get; set; }

        [JsonProperty("billCode")]
        public string BillCode { get; set; }

        [JsonProperty("signCloudMetaData")]
        public SignCloudMetaData SignCloudMetaData { get; set; }

        [JsonProperty("messagingMode")]
        public int MessagingMode { get; set; }

        [JsonProperty("sharedMode")]
        public int SharedMode { get; set; }

        [JsonProperty("xslTemplateUUID")]
        public string XslTemplateUUID { get; set; }

        [JsonProperty("xslTemplate")]
        public string XslTemplate { get; set; }

        [JsonProperty("xmlDocument")]
        public string XmlDocument { get; set; }

        [JsonProperty("p2pEnabled")]
        public bool P2pEnabled { get; set; }

        [JsonProperty("csrRequired")]
        public bool CsrRequired { get; set; }

        [JsonProperty("certificateRequired")]
        public bool CertificateRequired { get; set; }

        [JsonProperty("keepOldKeysEnabled")]
        public bool KeepOldKeysEnabled { get; set; }

        [JsonProperty("revokeOldCertificateEnabled")]
        public bool RevokeOldCertificateEnabled { get; set; }

        [JsonProperty("certificate")]
        public string Certificate { get; set; }

        [JsonProperty("multipleSigningFileData")]
        public List<MultipleSigningFileData> MultipleSigningFileData { get; set; }

        [JsonProperty("cloudCertificateID")]
        public long CloudCertificateID { get; set; }

        [JsonProperty("sad")]
        public string Sad { get; set; }

        [JsonProperty("sicTemplate")]
        public string SicTemplate { get; set; }

        [JsonProperty("sicTransactionInfo")]
        public string SicTransactionInfo { get; set; }

        [JsonProperty("subjectID")]
        public string SubjectID { get; set; }

        [JsonProperty("processID")]
        public string ProcessID { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("handwrittenSignature")]
        public string HandwrittenSignature { get; set; }

        [JsonProperty("sicResultCode")]
        public int SicResultCode { get; set; }
    }
}
