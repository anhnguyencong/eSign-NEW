using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SdkTester.dataModel
{
    
        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class SignCloudMetaData
        {
            [JsonProperty("singletonSigning")]
            public Dictionary<string, string> SingletonSigning { get; set; }

            [JsonProperty("counterSigning")]
            public Dictionary<string, string> CounterSigning { get; set; }
        }
}
