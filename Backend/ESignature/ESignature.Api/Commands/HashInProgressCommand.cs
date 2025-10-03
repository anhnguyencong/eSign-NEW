using ESignature.HashServiceLayer.Settings;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.Api.Commands
{
    public class HashInProgressCommand : IRequest<bool>
    {
        public HashRsspCloudSetting HashRsspCloudSetting { get; set; }

        // danh sách fullname branch kí để kiểm soát việc xuống dòng của tên khi kí
        public Branch BranchSetting { get; set; }
        public string FilePath { get; set; }
        public string FilePassword { get; set; }
        public string CompletedFileName { get; set; }
        public string CompletedFilePath { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Description { get; set; }
        public string PageSign { get; set; }
        public string VisiblePosition { get; set; }
    }
    public class HashInProgressCommandHandler : IRequestHandler<HashInProgressCommand, bool>
    {
        
        private readonly ILogger<HashInProgressCommandHandler> _logger;
        public HashInProgressCommandHandler(ILogger<HashInProgressCommandHandler> logger)
        {           
            _logger = logger;
        }

        public async Task<bool> Handle(HashInProgressCommand request, CancellationToken cancellationToken)
        {
            return true;
        }
    }
}
