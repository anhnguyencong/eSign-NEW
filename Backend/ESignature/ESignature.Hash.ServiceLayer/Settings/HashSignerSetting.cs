using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESignature.Hash.ServiceLayer.Settings
{
    public class HashSignerSetting
    {
        public List<HashRsspCloudSetting> Signers { get; set; }
    }

    public class HashRsspCloudSetting
    {
        public string SignerId { get; set; } //relyingParty
        public string AgreementUUID { get; set; } //agreementUUID = "55ACEDBF-2AF9-4917-805E-87359394F763"
        public string CredentialID { get; set; } // có vẻ ko xài
        public string PassCode { get; set; }
        public string RestUrl { get; set; } // URL = https://rssp.fptdev.site/eSignCloud/restapi/
        public string Username { get; set; } //relyingPartyUser
        public string Password { get; set; } //relyingPartyPassword
        public string KeyStore { get; set; } //relyingPartyKeyStore
        public string KeyStorePassword { get; set; } //relyingPartyKeyStorePassword
        public string Signature { get; set; } //relyingPartySignature
    }
}
