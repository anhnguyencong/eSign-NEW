using Microsoft.AspNetCore.Hosting;

namespace ESignature.ServiceLayer.Services.OnStartup
{
    public class ServiceData
    {
        public bool IsStop { get; set; } = false;
    }
}