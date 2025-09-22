using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESignature.Core.Settings
{
    public class RabbitMQSettings
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int ConsumerDispatchConcurrency { get; set; }
        public int PrefetchBasicQos { get; set; }
        public string PendingJobQueueName { get; set; }
        public string InProgressJobQueueName { get; set; }
        public string CallBackJobQueueName { get; set; }
    }
}
