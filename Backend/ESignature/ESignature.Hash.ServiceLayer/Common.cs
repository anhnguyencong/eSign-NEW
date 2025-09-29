using SdkTester.dataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESignature.Hash.ServiceLayer
{
    public static class Common
    {
        public static List<SignCloudReq> SignCloudReqList { get; set; } = new List<SignCloudReq>();
        public static HashSet<Guid> ExistingBaseProductDetailsIds = new HashSet<Guid>();
    }
}
