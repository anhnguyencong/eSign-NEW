using System.Text;

namespace ESignature.HashServiceLayer.ESignCloud
{
    public static class Utils
    {
        public static string GetPKCS1Signature(string data, string key, string passkey)
        {
            MakeSignature mks = new MakeSignature(data, key, passkey);
            return mks.GetSignature();
        }

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        internal static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static byte[] Base64Encode(byte[] rawData)
        {
            var data = System.Convert.ToBase64String(rawData);
            return Encoding.UTF8.GetBytes(data);
        }

        public static byte[] Base64Decode(byte[] base64EncodedData)
        {
            var data = System.Text.Encoding.UTF8.GetString(base64EncodedData);
            var base64EncodedBytes = System.Convert.FromBase64String(data); 
            return base64EncodedBytes;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}