using AutoMapper;
using ESignature.Core.BaseDtos;
using ESignature.Core.Infrastructure;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Services.OnStartup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.ServiceLayer.Services.Commands.Jobs
{
    public class RetryJobCommand : IRequest<ResponseDto<bool>>
    {
        public string JobId { get; set; }
    }

    public class RetryJobCommandHandler : IRequestHandler<RetryJobCommand, ResponseDto<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Job> _jobRepo;
        private readonly ApiSourceData _apiSourceData;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RetryJobCommandHandler(IMapper mapper, IUnitOfWork unitOfWork,
            ApiSourceData apiSourceData, IWebHostEnvironment webHostEnvironment)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _jobRepo = _unitOfWork.GetRepository<Job>();
            _apiSourceData = apiSourceData;
        }

        public async Task<ResponseDto<bool>> Handle(RetryJobCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseDto<bool>();
            var job = await _jobRepo.FirstOrDefaultAsync(q => q.Id.ToString() == request.JobId && q.Status == JobStatus.Failed);
            if (job != null)
            {
                job.Priority = JobPriority.P1;
                job.Status = JobStatus.Pending;
                _jobRepo.Update(job);
                await _unitOfWork.SaveChangesAsync();
            }

            response.Result = true;
            return response;
        }
    }
}
