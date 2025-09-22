using System.Collections.Generic;

namespace ESignature.ServiceLayer.Settings
{
    public class SignerSetting
    {
        public List<RsspCloudSetting> Signers { get; set; }
    }

    public class RsspCloudSetting
    {
        public string SignerId { get; set; }
        public string AgreementUUID { get; set; }
        public string CredentialID { get; set; }
        public string PassCode { get; set; }
        public string RestUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string KeyStore { get; set; }
        public string KeyStorePassword { get; set; }
        public string Signature { get; set; }
    }
}