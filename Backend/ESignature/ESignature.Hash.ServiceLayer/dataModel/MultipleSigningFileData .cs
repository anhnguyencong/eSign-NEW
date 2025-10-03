using Newtonsoft.Json;

namespace SdkTester.dataModel
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MultipleSigningFileData
    {
        [JsonProperty("signingFileData")]
        public byte[] SigningFileData { get; set; }

        [JsonProperty("signingFileName")]
        public string SigningFileName { get; set; }

        [JsonProperty("mimeType")]
        public string MimeType { get; set; }

        [JsonProperty("xslTemplate")]
        public string XslTemplate { get; set; }

        [JsonProperty("xmlDocument")]
        public string XmlDocument { get; set; }

        [JsonProperty("signCloudMetaData")]
        public SignCloudMetaData SignCloudMetaData { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}
