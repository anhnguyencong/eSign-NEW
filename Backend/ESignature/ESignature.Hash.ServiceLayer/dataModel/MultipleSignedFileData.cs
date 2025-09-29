using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SdkTester.dataModel
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MultipleSignedFileData
    {
        [JsonProperty("signedFileData")]
        public byte[] SignedFileData { get; set; }

        [JsonProperty("mimeType")]
        public string MimeType { get; set; }

        [JsonProperty("signedFileName")]
        public string SignedFileName { get; set; }

        [JsonProperty("signedFileUUID")]
        public string SignedFileUUID { get; set; }

        [JsonProperty("dmsMetaData")]
        public string DmsMetaData { get; set; }

        [JsonProperty("signatureValue")]
        public string SignatureValue { get; set; }
    }
}
