using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.IO.Compression;

namespace SdkTester.eSign
{
    public static class Utils
    {
        public static string detectOfficeFileType(byte[] fileContent)
        {
            try
            {
                using (var bais = new MemoryStream(fileContent))
                using (var zis = new ZipArchive(bais, ZipArchiveMode.Read))
                {
                    foreach (var entry in zis.Entries)
                    {
                        string name = entry.FullName;

                        if (name.StartsWith("word/", StringComparison.OrdinalIgnoreCase))
                            return "docx";
                        else if (name.StartsWith("xl/", StringComparison.OrdinalIgnoreCase))
                            return "xlsx";
                        else if (name.StartsWith("ppt/", StringComparison.OrdinalIgnoreCase))
                            return "pptx";
                    }
                }
            }
            catch (InvalidDataException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }
        private static readonly char[] HEX_ARRAY = "0123456789ABCDEF".ToCharArray();

        public static string GetSignature(string data, string p12Path, string p12Password)
        {
            using var cert = new X509Certificate2(p12Path, p12Password, X509KeyStorageFlags.Exportable);
            using var rsa = cert.GetRSAPrivateKey();
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }

        public static string GetPKCS1Signature(string data, string p12Path, string p12Password)
        {
            using var cert = new X509Certificate2(p12Path, p12Password, X509KeyStorageFlags.Exportable);
            using var rsa = cert.GetRSAPrivateKey();
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }

        //public static RSA GetPrivateKeyFromPem(string pem)
        //{
        //    var cleaned = pem
        //        .Replace("-----BEGIN PRIVATE KEY-----", "")
        //        .Replace("-----END PRIVATE KEY-----", "")
        //        .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
        //        .Replace("-----END RSA PRIVATE KEY-----", "")
        //        .Replace("\n", "")
        //        .Replace("\r", "");

        //    var bytes = Convert.FromBase64String(cleaned);
        //    var rsa = RSA.Create();
        //    rsa.ImportPkcs8PrivateKey(bytes, out _);
        //    return rsa;
        //}

        //public static byte[] DecryptRSA(string base64EncryptedAESKey, RSA privateKey)
        //{
        //    try
        //    {
        //        var encryptedBytes = Base64UrlDecode(base64EncryptedAESKey);
        //        return privateKey.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("RSA Decrypt error: " + ex.Message);
        //        return null;
        //    }
        //}

        //public static string AesDecrypt(string base64EncryptedData, byte[] iv, byte[] key)
        //{
        //    try
        //    {
        //        using var aes = Aes.Create();
        //        aes.Key = key;
        //        aes.IV = iv;
        //        aes.Mode = CipherMode.CBC;
        //        aes.Padding = PaddingMode.PKCS7;

        //        using var decryptor = aes.CreateDecryptor();
        //        var encryptedBytes = Base64UrlDecode(base64EncryptedData);
        //        var decrypted = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        //        return Encoding.UTF8.GetString(decrypted);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("AES Decrypt error: " + ex.Message);
        //        return null;
        //    }
        //}

        //public static HashSet<string> GetListFile(string dir)
        //{
        //    var files = new HashSet<string>();
        //    if (Directory.Exists(dir))
        //    {
        //        foreach (var path in Directory.GetFiles(dir))
        //        {
        //            files.Add(Path.GetFileName(path));
        //        }
        //    }
        //    return files;
        //}

        public static string PrintHexBinary(byte[] bytes)
        {
            var hex = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int v = bytes[i] & 0xFF;
                hex[i * 2] = HEX_ARRAY[v >> 4];
                hex[i * 2 + 1] = HEX_ARRAY[v & 0x0F];
            }
            return new string(hex);
        }

        //private static byte[] Base64UrlDecode(string input)
        //{
        //    input = input.Replace('-', '+').Replace('_', '/');
        //    switch (input.Length % 4)
        //    {
        //        case 2: input += "=="; break;
        //        case 3: input += "="; break;
        //    }
        //    return Convert.FromBase64String(input);
        //}
    }
}
