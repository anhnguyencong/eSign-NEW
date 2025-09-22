using AutoMapper;
using ESignature.Core.BaseDtos;
using ESignature.Core.Helpers;
using ESignature.Core.Infrastructure;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Services.OnStartup;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.ServiceLayer.Services.Commands
{
    public class UploadFileCommand : IRequest<ResponseDto<bool>>
    {
        public string SignerId { get; set; }
        public string BatchId { get; set; }
        public string RefId { get; set; }
        public string RefNumber { get; set; }
        public string JsonData { get; set; }
        public string CallBackUrl { get; set; }
        public bool NeedSign { get; set; } = false;
        public bool ConvertToPdf { get; set; } = false;
        public string FilePassword { get; set; }
        public string Description { get; set; }
        public DateTime? SignedDate { get; set; }
        public JobPriority? Priority { get; set; }
        public IFormFile File { get; set; }
        [JsonIgnore]
        public string PageSign { get; set; }
        [JsonIgnore]
        public string VisiblePosition { get; set; }

        [JsonIgnore]
        public string TokenKey { get; set; }
    }

    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ResponseDto<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Job> _jobRepo;
        private readonly ApiSourceData _apiSourceData;
        private readonly ILogger<UploadFileCommandHandler> _logger;

        public UploadFileCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, ApiSourceData apiSourceData, ILogger<UploadFileCommandHandler> logger)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _jobRepo = _unitOfWork.GetRepository<Job>();
            _apiSourceData = apiSourceData;
            _logger = logger;
        }

        public async Task<ResponseDto<bool>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseDto<bool>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            if (request.File == null)
            {
                response.Errors.Add(new ErrorDto
                {
                    Message = "File should be not empty"
                });
                return response;
            }
            else
            {
                if (!request.File.FileName.HasOfficeExtension())
                {
                    response.Errors.Add(new ErrorDto
                    {
                        Message = "only docx, xlsx, xls and pptx files are allowed"
                    });
                    return response;
                }
            }

            if (request.NeedSign)
            {
                var signer = _apiSourceData.GetSigner(request.SignerId);
                if (signer == null)
                {
                    response.Errors.Add(new ErrorDto
                    {
                        Message = "SignerId not found. Check SignerId and try again."
                    });
                    return response;
                }
            }

            if (!request.Priority.HasValue)
            {
                request.Priority = JobPriority.P10;
            }

            var apiSource = _apiSourceData.GetApiSource(request.TokenKey);
            var newJob = _mapper.Map<Job>(request);
            newJob.AppName = apiSource.Name;
            newJob.AppTokenKey = apiSource.Key;
            newJob.Status = JobStatus.Pending;

            var filePendingId = Guid.NewGuid();
            var filePendingName = filePendingId + Path.GetExtension(request.File.FileName);
            var mediaOriginalItem = new Media
            {
                Id = filePendingId,
                Name = request.File.FileName,
                ContentLength = request.File.Length,
                ContentType = request.File.ContentType,
                JobFileType = JobFileType.Original,
                Path = Path.Combine(apiSource.PendingPath, filePendingName)
            };
            newJob.Files.Add(mediaOriginalItem);
            await _jobRepo.AddAsync(newJob);

            string filePath = Path.Combine(apiSource.Folder, mediaOriginalItem.Path);
            var folder = Directory.GetParent(filePath);
            if (!Directory.Exists(folder.FullName))
            {
                Directory.CreateDirectory(folder.FullName);
            }
            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(fileStream);
            }

            await _unitOfWork.SaveChangesAsync();
            response.Result = true;

            stopwatch.Stop();
            _logger.LogWarning($"UploadFileCommand: {stopwatch.ElapsedMilliseconds} ms");

            return response;
        }
    }
}