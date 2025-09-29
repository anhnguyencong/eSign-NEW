using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SdkTester.dataModel
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CredentialData
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("pkcs1Signature")]
        public string Pkcs1Signature { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
