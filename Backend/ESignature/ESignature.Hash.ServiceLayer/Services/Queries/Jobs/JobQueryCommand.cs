using AutoMapper;
using AutoMapper.QueryableExtensions;
using ESignature.Core.BaseDtos;
using ESignature.Core.Helpers;
using ESignature.Core.Infrastructure;
using ESignature.Core.Infrastructure.Collections;
using ESignature.DAL;
using ESignature.DAL.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ESignature.HashServiceLayer.Services.Queries.Users
{
    public class JobQueryCommand : IRequest<ResponseDto<JobMonitorDto>>
    {
        public string TextSearch { get; set; }
        public string SourceName { get; set; }
        public string Status { get; set; }
        public string BatchId { get; set; }
        public IList<JobStatus> StatusIds { get; set; }
        public IList<CallBackStatus> CallbackStatusIds { get; set; }

        [Required]
        public int PageIndex { get; set; }

        [Required]
        public int PageSize { get; set; }
    }

    public class JobQueryCommandHandler : IRequestHandler<JobQueryCommand, ResponseDto<JobMonitorDto>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Job> _jobRepo;
        private readonly IRepository<Media> _mediaRepo;
        private readonly IHttpContextAccessor _httpContext;

        public JobQueryCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContext)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _httpContext = httpContext;
            _jobRepo = _unitOfWork.GetRepository<Job>();
            _mediaRepo = _unitOfWork.GetRepository<Media>();
        }

        public async Task<ResponseDto<JobMonitorDto>> Handle(JobQueryCommand request, CancellationToken cancellationToken)
        {
            var result = new ResponseDto<JobMonitorDto>();
            var query = _jobRepo.Query();
            if (!string.IsNullOrEmpty(request.TextSearch))
            {
                query = query.Where(q => q.AppName.Contains(request.TextSearch) ||
                                         q.Files.Any(t => t.Name.Contains(request.TextSearch) && t.JobFileType == JobFileType.Original) ||
                                         q.RefId.Contains(request.TextSearch) ||
                                         q.RefNumber.Contains(request.TextSearch));
            }

            if (request.StatusIds != null && request.StatusIds.Any())
            {
                query = query.Where(q => request.StatusIds.Contains(q.Status));
            }

            if (request.CallbackStatusIds != null && request.CallbackStatusIds.Any())
            {
                query = query.Where(q => request.CallbackStatusIds.Contains(q.CallBackStatus.Value));
            }

            if (!string.IsNullOrEmpty(request.BatchId))
            {
                query = query.Where(q => q.BatchId.Contains(request.BatchId));
            }

            if (!string.IsNullOrEmpty(request.SourceName))
            {
                query = query.Where(q => q.AppTokenKey == request.SourceName);
            }

            var items = await query.OrderBy(q => q.Status)
                                    .ThenBy(q => q.Priority)
                                    .ThenByDescending(x => x.CreatedDate)
                                    .ProjectTo<JobMonitorItemDto>(_mapper.ConfigurationProvider)
                                    .ToPagedListAsync(request.PageIndex, request.PageSize);

            foreach (var item in items.Items)
            {
                if (!string.IsNullOrEmpty(item.CompletedFileName))
                {
                    
                    var f = item.Files.FirstOrDefault(q => q.Name == item.CompletedFileName);
                    if (f != null)
                    {
                        item.CompletedFileUrl = ServiceExtensions.ToDownloadUrl(_httpContext, f.Id.ToString());
                    }
                }
            }

            var jobSummary = new JobSummaryDto();

            jobSummary.Total = query.Count();
            jobSummary.Completed = query.Count(q => q.Status == JobStatus.Completed);
            jobSummary.InProgress = query.Count(q => q.Status == JobStatus.Processing);
            jobSummary.Pending = query.Count(q => q.Status == JobStatus.Pending);
            jobSummary.Failed = query.Count(q => q.Status == JobStatus.Failed);
            jobSummary.CallbackComplete = query.Count(q => q.CallBackStatus == CallBackStatus.Completed);
            jobSummary.CallbackPending = query.Count(q => q.CallBackStatus == CallBackStatus.Pending);
            jobSummary.CallbackFailed = query.Count(q => q.CallBackStatus == CallBackStatus.Failed);

            result.Result = new JobMonitorDto();
            result.Result.Items = items;
            result.Result.JobSummary = jobSummary;

            return result;
        }
    }
}