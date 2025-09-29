using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdkTester.dataModel
{
    [Serializable]
    public class HashManagement
    {
        // Constants
        public const int HASH_TYPE_PDF = 0;
        public const int HASH_TYPE_OFFICE = 1;
        public const int HASH_TYPE_XML = 2;

        // Properties
        public string HashUUID { get; set; }
        public string HashValue { get; set; }
        public int HashType { get; set; }

        // Constructor
        public HashManagement(string hashValue, int hashType)
        {
            HashUUID = Guid.NewGuid().ToString(); // Tương đương UUID.randomUUID()
            HashValue = hashValue;
            HashType = hashType;
        }
    }
}
