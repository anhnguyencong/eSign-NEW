using ESignature.Core.BaseDtos;
using ESignature.Core.Infrastructure;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Services.Dtos;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.ServiceLayer.Services.Queries.Jobs
{
    public class JobBatchIdQueryCommand : IRequest<ResponseDto<IList<DropListDto>>>
    {
        public string AppName { get; set; }
    }

    public class JobBatchIdQueryCommandHandler : IRequestHandler<JobBatchIdQueryCommand, ResponseDto<IList<DropListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Job> _jobRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public JobBatchIdQueryCommandHandler(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _jobRepo = _unitOfWork.GetRepository<Job>();
        }

        public async Task<ResponseDto<IList<DropListDto>>> Handle(JobBatchIdQueryCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseDto<IList<DropListDto>>();

            var query = _jobRepo.Query();//.Where(q => q.Status == JobStatus.Pending);

            if (!string.IsNullOrEmpty(request.AppName))
            {
                query = query.Where(q => q.AppTokenKey == request.AppName);
            }

            var batchIds = await query.Select(s => new DropListDto
            {
                Key = s.BatchId,
                Value = s.BatchId
            }).Distinct().ToListAsync();

            response.Result = batchIds;
            return response;
        }
    }
}
