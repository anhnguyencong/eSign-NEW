using AutoMapper;
using ESignature.DAL.Models;
using ESignature.HashServiceLayer.Services.Commands;

namespace ESignature.HashServiceLayer.Mappers
{
    public class JobMapping : Profile
    {
        public JobMapping()
        {
            CreateMap<Job, JobHistory>();

            CreateMap<Job, JobDto>()
               .ForMember(dest => dest.Status, o => o.MapFrom(src => src.Status.ToString()))
               ;

            CreateMap<UploadFileCommand, Job>()
               .ForMember(dest => dest.Files, o => o.Ignore())
               .ForMember(dest => dest.Status, o => o.Ignore())
               .ForMember(dest => dest.RequestSignatureApiDate, o => o.Ignore())
               .ForMember(dest => dest.ResponseSignatureApiDate, o => o.Ignore())
               .ForMember(dest => dest.ApprovalDate, o => o.MapFrom(src => src.SignedDate))
               .ForMember(dest => dest.Description, o => o.MapFrom(src => src.Description))
               ;

            CreateMap<Job, JobMonitorItemDto>()
              .ForMember(dest => dest.DocumentName, o => o.MapFrom(src => src.Files.FirstOrDefault(q => q.JobFileType == DAL.JobFileType.Original).Name))
              .ForMember(dest => dest.CompletedFileName, o => o.MapFrom(src => src.NeedSign ? src.Files.FirstOrDefault(q => q.JobFileType == DAL.JobFileType.Completed).Name : src.Files.FirstOrDefault(q => q.JobFileType == DAL.JobFileType.Pending).Name))
              .ForMember(dest => dest.Status, o => o.MapFrom(src => src.Status.ToString()))
              .ForMember(dest => dest.SourceName, o => o.MapFrom(src => src.AppName))
              ;
        }
    }
}