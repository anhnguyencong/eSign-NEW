using ESignature.Api.BackgroundServices;
using ESignature.Core.Infrastructure;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Services.Commands;
using ESignature.ServiceLayer.Services.OnStartup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ESignature.Api
{
    public interface IHashInProgressSignService { }
    public class HashInProgressSignService : IHashInProgressSignService
    {
        private readonly ILogger<HashInProgressSignService> _logger;
        private readonly ApiSourceData _apiSource;
        private readonly IUnitOfWork _uow;
        private readonly IRepository<Job> _jobRepo;
        private readonly IRepository<Media> _mediaRepo;
        public HashInProgressSignService(ILogger<HashInProgressSignService> logger
            , ApiSourceData apiSource
            , IUnitOfWork uow)
        {
            _logger = _logger;
            _apiSource = apiSource;
            _uow = uow;
            _jobRepo = _uow.GetRepository<Job>();
            _mediaRepo = _uow.GetRepository<Media>();
        }

        public async Task<bool> CallHashInProgress(Guid id)
        {
            var item = await _jobRepo.FirstOrDefaultAsync(q => q.Id == id && q.Status == JobStatus.Processing, q => q.Include(k => k.Files));

            if (item == null) return false;

            var s = _apiSource.Sources.SingleOrDefault(q => q.Key == item.AppTokenKey);
            if (s != null)
            {
                item.RequestSignatureApiDate = DateTime.Now;
                try
                {
                    var pendingFile = item.Files.FirstOrDefault(q => q.JobFileType == JobFileType.Pending);
                    if (pendingFile != null)
                    {
                        var rsspCloudSetting = _apiSource.HashSigners.SingleOrDefault(q => q.SignerId == item.SignerId);

                        // danh sách fullname branch kí để kiểm soát việc xuống dòng của tên khi kí
                        var branchSetting = _apiSource.Branches.SingleOrDefault(q => q.SignerId == item.SignerId);
                        string completedFileName = Guid.NewGuid() + ".pdf";

                        var completedFilePath = Path.Combine(s.Folder, s.CompletedPath, completedFileName);

                        Stopwatch stopwatch2 = new Stopwatch();
                        stopwatch2.Start();

                        var command = new SignPDFCommand
                        {
                            FilePath = Path.Combine(s.Folder, pendingFile.Path),
                            FilePassword = item.FilePassword,
                            CompletedFileName = completedFileName,
                            CompletedFilePath = completedFilePath,
                            RsspCloudSetting = rsspCloudSetting,
                            // danh sách fullname branch kí để kiểm soát việc xuống dòng của tên khi kí
                            BranchSetting = branchSetting,
                            Description = item.Description,
                            ApprovalDate = item.ApprovalDate,
                            PageSign = item.PageSign,
                            VisiblePosition = item.VisiblePosition
                        };
                        await mediator.Send(command);

                        stopwatch2.Stop();
                        _logger.LogWarning($"InProgressJob_ProcessData_SignPDFCommand: {stopwatch2.ElapsedMilliseconds} ms");
                        var completedMedia = new Media
                        {
                            Name = completedFileName,
                            JobFileType = JobFileType.Completed,
                            Path = Path.Combine(s.CompletedPath, completedFileName),
                            ContentType = "application/pdf",
                            ContentLength = new FileInfo(completedFilePath).Length
                        };
                        item.Files.Add(completedMedia);
                        mediaRepo.ChangeEntityState(completedMedia, EntityState.Added);
                        item.Status = JobStatus.Completed;
                        item.ResponseSignatureApiDate = DateTime.Now;
                    }
                    else
                    {
                        _logger.LogError($"InProgressJob: pending file not found: jobid={item.Id}");
                    }
                }
                catch (Exception ex)
                {
                    item.Status = JobStatus.Failed;
                    item.Note = $"An error occurred in Progress Job: {ex.Message}";
                    _logger.LogError($"InProgressJob: " + ex);
                }
                finally
                {
                    if (item.Status == JobStatus.Completed)
                    {
                        item.CallBackStatus = CallBackStatus.Pending;
                    }
                    jobRepo.ChangeEntityState(item, EntityState.Modified);
                    await unitOfWork.SaveChangesAsync();
                    _logger.LogWarning($"InProgress Job is completed = {item.Id}");
                }
            }
            else
            {
                _logger.LogError($"InProgressJob: apiSourceData not found: jobid={item.Id}");
            }
            return true;
        }
    }
}
