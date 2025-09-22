using AutoMapper;
using ESignature.Core.BaseDtos;
using ESignature.Core.Infrastructure;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Services.OnStartup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.ServiceLayer.Services.Commands.Jobs
{
    public class RetryCallbackByBatchIdCommand : IRequest<ResponseDto<bool>>
    {
        [Required]
        public string BatchId { get; set; }
    }

    public class RetryCallbackByBatchIdCommandHandler : IRequestHandler<RetryCallbackByBatchIdCommand, ResponseDto<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Job> _jobRepo;
        private readonly ApiSourceData _apiSourceData;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RetryCallbackByBatchIdCommandHandler(IMapper mapper, IUnitOfWork unitOfWork,
            ApiSourceData apiSourceData, IWebHostEnvironment webHostEnvironment)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _jobRepo = _unitOfWork.GetRepository<Job>();
            _apiSourceData = apiSourceData;
        }

        public async Task<ResponseDto<bool>> Handle(RetryCallbackByBatchIdCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseDto<bool>();
            var query = _jobRepo.Query().Where(q => q.CallBackStatus == CallBackStatus.Failed && q.BatchId == request.BatchId);

            var jobs = await query.ToListAsync();

            foreach (var job in jobs)
            {
                job.CallBackStatus = CallBackStatus.Pending;
                _jobRepo.Update(job);
            }
            await _unitOfWork.SaveChangesAsync();
            response.Result = true;
            return response;
        }

    }
}
