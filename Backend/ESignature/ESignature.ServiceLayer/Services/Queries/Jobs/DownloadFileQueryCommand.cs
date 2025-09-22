using ESignature.Core.BaseDtos;
using ESignature.Core.Infrastructure;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Services.OnStartup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.ServiceLayer.Services.Commands
{
    public class DownloadFileQueryCommand : IRequest<ResponseDto<MemoryStream>>
    {
        public Guid Id { get; set; }

        [JsonIgnore]
        public string TokenKey { get; set; }
    }

    public class DownloadFileCommandHandler : IRequestHandler<DownloadFileQueryCommand, ResponseDto<MemoryStream>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Media> _mediaRepo;
        private readonly ApiSourceData _apiSourceData;

        public DownloadFileCommandHandler(IUnitOfWork unitOfWork, ApiSourceData apiSourceData)
        {
            _unitOfWork = unitOfWork;
            _apiSourceData = apiSourceData;
            _mediaRepo = _unitOfWork.GetRepository<Media>();
        }

        public async Task<ResponseDto<MemoryStream>> Handle(DownloadFileQueryCommand request, CancellationToken cancellationToken)
        {
            var s = _apiSourceData.GetApiSource(request.TokenKey);
            var response = new ResponseDto<MemoryStream>();
            var media = await _mediaRepo.Query(q => q.Id == request.Id).FirstOrDefaultAsync();
            if (media != null)
            {
                var filePath = Path.Combine(s.Folder, media.Path);
                if (File.Exists(filePath))
                {
                    var memory = new MemoryStream();
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        await stream.CopyToAsync(memory);
                        response.Result = memory;
                    }
                    memory.Position = 0;
                }
                else
                {
                    response.Result = null;
                }
            }
            return response;
        }
    }
}