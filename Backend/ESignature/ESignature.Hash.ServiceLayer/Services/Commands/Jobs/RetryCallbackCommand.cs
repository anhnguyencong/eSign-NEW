using AutoMapper;
using ESignature.Core.BaseDtos;
using ESignature.Core.Infrastructure;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.HashServiceLayer.Services.OnStartup;
using MediatR;
using Microsoft.AspNetCore.Hosting;

namespace ESignature.HashServiceLayer.Services.Commands.Jobs
{
    public class RetryCallbackCommand : IRequest<ResponseDto<bool>>
    {
        public string JobId { get; set; }
    }

    public class RetryCallbackCommandHandler : IRequestHandler<RetryCallbackCommand, ResponseDto<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Job> _jobRepo;
        private readonly ApiSourceData _apiSourceData;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RetryCallbackCommandHandler(IMapper mapper, IUnitOfWork unitOfWork,
            ApiSourceData apiSourceData, IWebHostEnvironment webHostEnvironment)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _jobRepo = _unitOfWork.GetRepository<Job>();
            _apiSourceData = apiSourceData;
        }

        public async Task<ResponseDto<bool>> Handle(RetryCallbackCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseDto<bool>();
            var job = await _jobRepo.FirstOrDefaultAsync(q => q.Id.ToString() == request.JobId && q.CallBackStatus == CallBackStatus.Failed);
            if (job != null)
            {
                job.CallBackStatus = CallBackStatus.Pending;
                _jobRepo.Update(job);
                await _unitOfWork.SaveChangesAsync();
            }

            response.Result = true;
            return response;
        }
    }
}
