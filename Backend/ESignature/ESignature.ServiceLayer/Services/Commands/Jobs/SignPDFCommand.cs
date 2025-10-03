using ESignature.HashServiceLayer.Settings;
using ESignature.ServiceLayer.ESignCloud;
using ESignature.ServiceLayer.Settings;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.ServiceLayer.Services.Commands
{
    public class SignPDFCommand : IRequest<bool>
    {
        public RsspCloudSetting RsspCloudSetting { get; set; }

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

    public class SignPDFCommandHandler : IRequestHandler<SignPDFCommand, bool>
    {
        private readonly ESignCloudFunction _esgnCloudFunction;
        private readonly ILogger<SignPDFCommandHandler> _logger;
        public SignPDFCommandHandler(ESignCloudFunction esgnCloudFunction, ILogger<SignPDFCommandHandler> logger)
        {
            _esgnCloudFunction = esgnCloudFunction;
            _logger = logger;
        }

        public async Task<bool> Handle(SignPDFCommand request, CancellationToken cancellationToken)
        {
            //var files = new List<FileDto>
            //{
            //    new FileDto
            //    {
            //        FilePendingPath = request.FilePath,
            //        Password = request.FilePassword
            //    }
            //};
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //try
            //{
            //    var list = await _esgnCloudFunction.SignPdf(request.RsspCloudSetting, files, request.Description, request.ApprovalDate, request.VisiblePosition, request.PageSign, request.BranchSetting);
            //    if (list != null && list.Any())
            //    {
            //        File.WriteAllBytes(request.CompletedFilePath, list.First());
            //    }
            //}
            //catch (Exception ex)
            //{                
            //}
            //stopwatch.Stop();
            //_logger.LogWarning($"_esgnCloudFunction.SignPdf: {stopwatch.ElapsedMilliseconds} ms");

            return true;
        }
    }
}