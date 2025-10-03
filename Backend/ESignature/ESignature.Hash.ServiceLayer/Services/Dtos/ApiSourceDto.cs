namespace ESignature.HashServiceLayer.Services.Dtos
{
    public class ApiSourceDto
    {
        public IList<ApiSourceItemDto> Apps { get; set; }
    }

    public class ApiSourceItemDto
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Folder { get; set; }
        public string IpAddress { get; set; }
        public string PendingPath { get { return "pending"; } }
        public string CompletedPath { get { return "completed"; } }
    }
}