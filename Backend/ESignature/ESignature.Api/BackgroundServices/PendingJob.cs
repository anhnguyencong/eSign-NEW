using ESignature.Core.Infrastructure;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Services.OnStartup;
using ESignature.ServiceLayer.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.Api.BackgroundServices
{
    public class PendingJob : BackgroundService
    {
        private readonly int _maxThreads;
        private readonly ILogger<PendingJob> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ApiSourceData _apiSource;
        private readonly ServiceData _service;

        public PendingJob(IServiceScopeFactory serviceScopeFactory, ApiSourceData apiSource,
                          ILogger<PendingJob> logger, ServiceData service, IOptions<ESignatureSetting> options)
        {
            _logger = logger;
            _service = service;
            _apiSource = apiSource;
            _serviceScopeFactory = serviceScopeFactory;
            _maxThreads = options.Value.MaxThreads;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => Start(stoppingToken));
            return Task.CompletedTask;
        }

        private async Task Start(CancellationToken stoppingToken)
        {
            _logger.LogWarning("Pending Job started...");
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
                        _logger.LogWarning($"PendingJob_ProcessData: {stopwatch.ElapsedMilliseconds} ms");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"PendingJob : " + ex);
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
        }

        private async Task ProcessData()
        {
            Stopwatch stopwatch1 = new Stopwatch();
            stopwatch1.Start();

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var jobRepo = unitOfWork.GetRepository<Job>();
                var mediaRepo = unitOfWork.GetRepository<Media>();
                var count = await jobRepo.CountAsync(q => q.Status == JobStatus.Processing);
                var todo = _maxThreads - count;

                if (todo > 0)
                {
                    var items = await jobRepo.Query(q => q.Status == JobStatus.Pending, q => q.Include(k => k.Files), false)
                                             .OrderBy(q => q.Priority)
                                             .ThenByDescending(q => q.CreatedDate)
                                             .Take(todo)
                                             .ToListAsync();
                    if (!items.Any())
                    {
                        return;
                    }

                    foreach (var item in items)
                    {
                        try
                        {
                            Stopwatch stopwatch2 = new Stopwatch();
                            stopwatch2.Start();

                            var originalItem = item.Files.First(q => q.JobFileType == JobFileType.Original);
                            if (item.ConvertToPdf)
                            {
                                var extension = Path.GetExtension(originalItem.Name);
                                if (extension == ".pdf")
                                {
                                    var mediaPendingItem = new Media
                                    {
                                        Name = originalItem.Name,
                                        ContentLength = originalItem.ContentLength,
                                        ContentType = originalItem.ContentType,
                                        JobFileType = JobFileType.Pending,
                                        Path = originalItem.Path
                                    };
                                    item.Files.Add(mediaPendingItem);
                                    await mediaRepo.AddAsync(mediaPendingItem);
                                }
                                else
                                {
                                    var mediaPendingItem = ConvertFileOfficeToPdf(item.AppTokenKey, originalItem, item.FilePassword);
                                    item.Files.Add(mediaPendingItem);
                                    await mediaRepo.AddAsync(mediaPendingItem);
                                }
                            }
                            else
                            {
                                var mediaPendingItem = new Media
                                {
                                    Name = originalItem.Name,
                                    ContentLength = originalItem.ContentLength,
                                    ContentType = originalItem.ContentType,
                                    JobFileType = JobFileType.Pending,
                                    Path = originalItem.Path
                                };
                                item.Files.Add(mediaPendingItem);
                                await mediaRepo.AddAsync(mediaPendingItem);
                            }

                            if (item.NeedSign)
                            {
                                item.Status = JobStatus.Processing;
                                _logger.LogWarning($"PendingJob Processing: id={item.Id}, RefId={item.RefId}, BatchId={item.BatchId}");
                            }
                            else
                            {
                                item.Status = JobStatus.Completed;
                                item.CallBackStatus = CallBackStatus.Pending;
                                _logger.LogWarning($"Completed: id={item.Id}, RefId={item.RefId}, BatchId={item.BatchId}");
                            }

                            stopwatch2.Stop();
                            _logger.LogWarning($"ProcessData_2: {stopwatch2.ElapsedMilliseconds} ms");
                       }
                        catch (Exception ex)
                        {
                            item.Status = JobStatus.Failed;
                            item.Note = $"An error occurred in Pending Job: {ex.Message}";
                            _logger.LogError($"PendingJob: " + ex);
                        }
                        finally
                        {
                            jobRepo.Update(item);
                            await unitOfWork.SaveChangesAsync();
                        }
                    }
                }
            }

            stopwatch1.Stop();
            _logger.LogWarning($"ProcessData_1: {stopwatch1.ElapsedMilliseconds} ms");
        }

        private Media ConvertFileOfficeToPdf(string appTokenKey, Media originalFile, string password)
        {
            File.AppendAllText(@"d:\esign.log", $"ConvertFileOfficeToPdf_begin:{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}");

            var s = _apiSource.Sources.SingleOrDefault(q => q.Key == appTokenKey);
            var fileId = Guid.NewGuid();
            var fileName = fileId + ".pdf";
            var filePath = Path.Combine(s.Folder, s.PendingPath, fileName);
            var originalFilePath = Path.Combine(s.Folder, originalFile.Path);
            var extension = Path.GetExtension(originalFile.Name);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.ToLower();
            }
            if (extension == ".doc" || extension == ".docx")
            {
                var loadOptions = new Aspose.Words.Loading.LoadOptions(password);
                var doc = new Aspose.Words.Document(originalFilePath, loadOptions);
                File.AppendAllText(@"d:\esign.log", $"ConvertFileOfficeToPdf_2.1:{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}");
                doc.Save(filePath, Aspose.Words.SaveFormat.Pdf);
                File.AppendAllText(@"d:\esign.log", $"ConvertFileOfficeToPdf_2.2:{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}");

            }
            else if (extension == ".xls" || extension == ".xlsx")
            {
                var loadOptions = new Aspose.Cells.LoadOptions();
                loadOptions.Password = password;
                var book = new Aspose.Cells.Workbook(originalFilePath, loadOptions);
                book.Save(filePath, Aspose.Cells.SaveFormat.Auto);
            }
            else if (extension == ".ppt" || extension == ".pptx")
            {
                var loadOptions = new Aspose.Slides.LoadOptions();
                loadOptions.Password = password;
                var presentation = new Aspose.Slides.Presentation(originalFilePath, loadOptions);
                presentation.Save(filePath, Aspose.Slides.Export.SaveFormat.Pdf);
            }

            var media = new Media
            {
                Id = fileId,
                Name = fileName,
                JobFileType = JobFileType.Pending,
                Path = Path.Combine(s.PendingPath, fileName),
                ContentType = "application/pdf",
                ContentLength = new FileInfo(filePath).Length
            };
            File.AppendAllText(@"d:\esign.log", $"ConvertFileOfficeToPdf_end:{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}");

            return media;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}