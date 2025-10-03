using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ESignature.HashServiceLayer.ESignCloud
{
    public class MakeSignature
    {
        private string data;
        private string key;
        private string passKey;

        public MakeSignature(string data, string PriKeyPath, string PriKeyPass)
        {
            this.data = data;
            key = PriKeyPath;
            passKey = PriKeyPass;
        }

        public string GetSignature()
        {
            RSACryptoServiceProvider key = GetKey();
            return Sign(data, key);
        }

        public static string Sign(string content, RSACryptoServiceProvider rsa)
        {
            RSACryptoServiceProvider crsa = rsa;
            byte[] Data = Encoding.UTF8.GetBytes(content);
            byte[] signData = crsa.SignData(Data, "sha1");
            return Convert.ToBase64String(signData);
        }

        private RSACryptoServiceProvider GetKey()
        {
            X509Certificate2 cert2 = new X509Certificate2(key, passKey,
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable);
            RSACryptoServiceProvider rsa = GetPrivateKeyRsaCryptoServiceProvider(cert2);
            return rsa;
        }

        private static RSACryptoServiceProvider GetPrivateKeyRsaCryptoServiceProvider(X509Certificate2 certificate)
        {
            var rsa = certificate.GetRSAPrivateKey();
            var rsaParameters = rsa.ExportParameters(true);
            var csp = new RSACryptoServiceProvider(rsa.KeySize);
            csp.ImportParameters(rsaParameters);
            return csp;
        }
    }
}