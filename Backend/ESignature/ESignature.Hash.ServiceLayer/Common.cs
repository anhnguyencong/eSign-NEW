using SdkTester.dataModel;

namespace ESignature.HashServiceLayer
{
    public static class Common
    {
        public static List<SignCloudReq> SignCloudReqList { get; set; } = new List<SignCloudReq>();
        public static HashSet<Guid> ExistingBaseProductDetailsIds = new HashSet<Guid>();
    }
}
