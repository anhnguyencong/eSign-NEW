namespace ESignature.HashServiceLayer.Settings
{
    public class HashSignerSetting
    {
        public List<HashRsspCloudSetting> Signers { get; set; }
    }

    public class HashRsspCloudSetting
    {
        public string SignerId { get; set; } // này là signerId = tên của đơn vị kí, sử dụng riêng cho bancas
        public string SignerName { get; set; }//relyingParty
        public string AgreementUUID { get; set; } //agreementUUID = "55ACEDBF-2AF9-4917-805E-87359394F763"
        public string CredentialID { get; set; } // có vẻ ko xài
        public string PassCode { get; set; } //authorizeCode = "12345678"
        public string RestUrl { get; set; } // URL = https://rssp.fptdev.site/eSignCloud/restapi/
        public string Username { get; set; } //relyingPartyUser
        public string Password { get; set; } //relyingPartyPassword
        public string KeyStore { get; set; } //relyingPartyKeyStore
        public string KeyStorePassword { get; set; } //relyingPartyKeyStorePassword
        public string Signature { get; set; } //relyingPartySignature

        public string ESignCloudClientPort { get; set; } // ESignCloudClient eSignCloudClient = new ESignCloudClient("9090");
    }
}
