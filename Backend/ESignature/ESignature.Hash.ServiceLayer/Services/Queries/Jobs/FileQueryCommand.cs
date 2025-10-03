using AutoMapper;
using AutoMapper.QueryableExtensions;
using ESignature.Core.BaseDtos;
using ESignature.Core.Helpers;
using ESignature.Core.Infrastructure;
using ESignature.DAL.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace ESignature.HashServiceLayer.Services.Queries.Users
{
    public class FileQueryCommand : IRequest<ResponseDto<IList<JobDto>>>
    {
        [JsonIgnore]
        [BindNever]
        public string TokenKey { get; set; }
    }

    public class FileQueryCommandHandler : IRequestHandler<FileQueryCommand, ResponseDto<IList<JobDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Job> _jobRepo;
        private readonly IHttpContextAccessor _httpContext;

        public FileQueryCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContext)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _httpContext = httpContext;
            _jobRepo = _unitOfWork.GetRepository<Job>();
        }

        public async Task<ResponseDto<IList<JobDto>>> Handle(FileQueryCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseDto<IList<JobDto>>();
            var items = await _jobRepo.Query(q => q.AppTokenKey == request.TokenKey)
                                      .OrderByDescending(q => q.Id)
                                      .ProjectTo<JobDto>(_mapper.ConfigurationProvider)
                                      .ToListAsync();
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.CompletedFileName))
                {
                    var files = item.CompletedFileName.Split(';');
                    for (int i = 0; i < files.Length; i++)
                    {
                        var f = files[i];
                        item.CompletedFileUrls.Add(new CompletedFileDto
                        {
                            FileName = $"File {i + 1}",
                            FileUrl = ServiceExtensions.ToDownloadUrl(_httpContext, f)
                        });
                    }
                }
            }
            response.Result = items;
            return response;
        }
    }
}