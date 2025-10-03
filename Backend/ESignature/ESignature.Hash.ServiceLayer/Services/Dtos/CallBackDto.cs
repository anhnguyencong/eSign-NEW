namespace ESignature.HashServiceLayer.Services.Dtos
{
    public class CallBackDto
    {
        public string BatchId { get; set; }
        public string RefId { get; set; }
        public string FileCompletedUrl { get; set; }
        public string JsonData { get; set; }
        public string Status { get; set; }
    }
}