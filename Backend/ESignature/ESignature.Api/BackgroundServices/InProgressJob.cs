using ESignature.Core.Infrastructure;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Services.Commands;
using ESignature.ServiceLayer.Services.OnStartup;
using ESignature.ServiceLayer.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.Api.BackgroundServices
{
    public class InProgressJob : BackgroundService
    {
        private readonly ILogger<InProgressJob> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ApiSourceData _apiSource;
        private readonly int _maxThreads;
        private readonly ServiceData _service;

        public InProgressJob(IServiceScopeFactory serviceScopeFactory, IOptions<ESignatureSetting> options,
            ApiSourceData apiSource, ServiceData service, ILogger<InProgressJob> logger)
        {
            _logger = logger;
            _service = service;
            _apiSource = apiSource;
            _maxThreads = options.Value.MaxThreads;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => Start(stoppingToken));
            return Task.CompletedTask;
        }

        private async Task Start(CancellationToken stoppingToken)
        {
            _logger.LogWarning("InProgress Job started...");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_apiSource.Sources != null && !_service.IsStop)
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        await ProcessData();

                        stopwatch.Stop();
                        _logger.LogWarning($"InProgressJob_ProcessData: {stopwatch.ElapsedMilliseconds} ms");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"InProgressJob: " + ex);
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
        }

        private async Task ProcessData()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var jobRepo = unitOfWork.GetRepository<Job>();
                var mediaRepo = unitOfWork.GetRepository<Media>();
                var items = await jobRepo.Query(q => q.Status == JobStatus.Processing, q => q.Include(k => k.Files))
                                         .OrderBy(q => q.CreatedDate)
                                         .Take(_maxThreads)
                                         .ToListAsync();
                if (!items.Any())
                {
                    return;
                }

                foreach (var item in items)
                {
                    var s = _apiSource.Sources.SingleOrDefault(q => q.Key == item.AppTokenKey);

                    Stopwatch stopwatch1 = new Stopwatch();
                    stopwatch1.Start();


                    if (s != null)
                    {
                        item.RequestSignatureApiDate = DateTime.Now;
                        try
                        {
                            var pendingFile = item.Files.FirstOrDefault(q => q.JobFileType == JobFileType.Pending);
                            if (pendingFile != null)
                            {
                                var rsspCloudSetting = _apiSource.Signers.SingleOrDefault(q => q.SignerId == item.SignerId);
                                
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

                    stopwatch1.Stop();
                    _logger.LogWarning($"InProgressJob_ProcessData: {stopwatch1.ElapsedMilliseconds} ms");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}