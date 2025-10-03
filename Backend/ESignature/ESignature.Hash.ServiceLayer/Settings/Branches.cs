namespace ESignature.HashServiceLayer.Settings
{
    public class BranchesSetting
    {
        public List<Branch> Branches { get; set; }
    }
    public class Branch
    {
        public string SignerId { get; set; }
        public string FullName { get; set; }
    }
}
