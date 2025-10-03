using AutoMapper;
using ESignature.Core.BaseDtos;
using ESignature.Core.Infrastructure;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.HashServiceLayer.Services.OnStartup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ESignature.HashServiceLayer.Services.Commands.Jobs
{
    public class UpdateJobPriorityCommand : IRequest<ResponseDto<bool>>
    {
        [Required]
        public JobUpdatePriorityType UpdateType { get; set; }
        [Required]
        public string Id { get; set; }
        [Required]
        public JobPriority Priority { get; set; }
    }

    public class UpdateJobPriorityCommandHandler : IRequestHandler<UpdateJobPriorityCommand, ResponseDto<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Job> _jobRepo;
        private readonly ApiSourceData _apiSourceData;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UpdateJobPriorityCommandHandler(IMapper mapper, IUnitOfWork unitOfWork,
            ApiSourceData apiSourceData, IWebHostEnvironment webHostEnvironment)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _jobRepo = _unitOfWork.GetRepository<Job>();
            _apiSourceData = apiSourceData;
        }

        public async Task<ResponseDto<bool>> Handle(UpdateJobPriorityCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseDto<bool>();
            //set priorty by batchId
            if (request.UpdateType == JobUpdatePriorityType.BatchId)
            {
                var jobs = await _jobRepo.Query().Where(q => q.BatchId == request.Id && q.Status == JobStatus.Pending)
                    .ToListAsync(); ;
                
                foreach (var job in jobs)
                {
                    job.Priority = request.Priority;
                    _jobRepo.Update(job);
                }
            }
            else //set priority by job
            {
                var job = await _jobRepo.FirstOrDefaultAsync(q => q.Id.ToString() == request.Id && q.Status == JobStatus.Pending);
                if (job != null)
                {
                    job.Priority = request.Priority;
                    _jobRepo.Update(job);
                }
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            response.Result = true;
            return response;
        }
    }
}
