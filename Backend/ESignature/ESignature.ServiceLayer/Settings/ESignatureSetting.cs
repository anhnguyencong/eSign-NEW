namespace ESignature.ServiceLayer.Settings
{
    public class ESignatureSetting
    {
        public int MaxThreads { get; set; }
        public int MaxDays { get; set; }
        public string HostUrl { get; set; }
        public bool IPAddressAuthentication { get; set; }
    }
}