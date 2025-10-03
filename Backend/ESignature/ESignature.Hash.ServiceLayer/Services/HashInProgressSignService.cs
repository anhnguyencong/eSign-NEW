//using ESignature.Api.BackgroundServices;
using ESignature.Core.Infrastructure;
using ESignature.Core.Settings;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.HashServiceLayer.Messages;
using ESignature.HashServiceLayer.Services.Commands;
using ESignature.HashServiceLayer.Services.OnStartup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ESignature.HashServiceLayer.Services
{
    public interface IHashInProgressSignService
    {
        Task<bool> CallHashInProgress(Guid id);
    }
    public class HashInProgressSignService : IHashInProgressSignService
    {
        private readonly ILogger<HashInProgressSignService> _logger;
        private readonly ApiSourceData _apiSource;
        private IUnitOfWork _uow;
        private IRepository<Job> _jobRepo;
        private IRepository<Media> _mediaRepo;
        private readonly IMediator _mediator;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMessagePublisher _publisher;
        private RabbitMQSettings _rabbitMQSettings;
        private readonly IConfiguration _config;

        public HashInProgressSignService(ILogger<HashInProgressSignService> logger
            , IConfiguration config
            , ApiSourceData apiSource
            , IServiceScopeFactory serviceScopeFactory
            , IMediator mediator
            , IMessagePublisher publisher)
        {
            _logger = logger;
            _apiSource = apiSource;
            _config = config;
            _rabbitMQSettings = _config.GetSection("LogRabbitMQSettings").Get<RabbitMQSettings>() ?? new RabbitMQSettings();
            _serviceScopeFactory = serviceScopeFactory;
            _mediator = mediator;
            _publisher = publisher;
        }

        public async Task<bool> CallHashInProgress(Guid id)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                _jobRepo = _uow.GetRepository<Job>();
                _mediaRepo = _uow.GetRepository<Media>();
                var item = await _jobRepo.FirstOrDefaultAsync(q => q.Id == id
                                            && q.Status == JobStatus.Processing
                                            //SentToMessageBroker is null OR false thì mới xử lý, tránh bị gọi lại khi đã gửi message thành công
                                            && (q.SentToMessageBroker != null && q.SentToMessageBroker > 0), q => q.Include(k => k.Files));

                if (item == null)
                {
                    _logger.LogWarning($"CallHashInProgress: {id} not exists");
                    return false;
                }


                var s = _apiSource.Sources.SingleOrDefault(q => q.Key == item.AppTokenKey);
                if (s != null)
                {
                    item.RequestSignatureApiDate = DateTime.Now;
                    try
                    {
                        bool pendingRes = await ProcessPendingFile(item);
                        _logger.LogWarning($"CallHashInProgress Job: jobid={item.Id}");

                        if (!pendingRes)
                        {
                            _jobRepo.Update(item);
                            await _uow.SaveChangesAsync();
                            return false;
                        }
                        if (item.NeedSign)
                        {
                            _logger.LogWarning($"CallHashInProgress_Sign: jobid={item.Id}");

                            var pendingFile = item.Files.FirstOrDefault(q => q.JobFileType == JobFileType.Pending);
                            if (pendingFile != null)
                            {
                                var rsspCloudSetting = _apiSource.HashSigners.SingleOrDefault(q => q.SignerId == item.SignerId);

                                // danh sách fullname branch kí để kiểm soát việc xuống dòng của tên khi kí
                                var branchSetting = _apiSource.Branches.SingleOrDefault(q => q.SignerId == item.SignerId);
                                string completedFileName = Guid.NewGuid() + ".pdf";
                                DirectoryInfo directoryInfo = new DirectoryInfo(pendingFile.Path);
                                string secondParent = directoryInfo.Parent.Name; // "10"
                                string thirdParent = directoryInfo.Parent.Parent.Name; // "2025"
                                //var dateFolder = Path.GetFileName(Path.GetDirectoryName(pendingFile.Path));
                                var completedFilePath = Path.Combine(s.Folder, s.CompletedPath, thirdParent, secondParent, completedFileName);
                                var saveCompletedFilePath = Path.Combine(s.CompletedPath, thirdParent, secondParent, completedFileName);
                                Stopwatch stopwatch2 = new Stopwatch();
                                stopwatch2.Start();

                                var command = new SignHashPDFCommand
                                {
                                    SignerId = item.SignerId,
                                    FilePath = Path.Combine(s.Folder, pendingFile.Path),
                                    FilePassword = item.FilePassword,
                                    CompletedFileName = completedFileName,
                                    CompletedFilePath = completedFilePath,
                                    HashRsspCloudSetting = rsspCloudSetting,
                                    // danh sách fullname branch kí để kiểm soát việc xuống dòng của tên khi kí
                                    BranchSetting = branchSetting,
                                    Description = item.Description,
                                    ApprovalDate = item.ApprovalDate,
                                    PageSign = item.PageSign,
                                    VisiblePosition = item.VisiblePosition
                                };
                                await _mediator.Send(command);

                                stopwatch2.Stop();
                                _logger.LogWarning($"CallHashInProgress_SignHashPDFCommand: {stopwatch2.ElapsedMilliseconds} ms");
                                var completedMedia = new Media
                                {
                                    Name = completedFileName,
                                    JobFileType = JobFileType.Completed,
                                    Path = saveCompletedFilePath,
                                    ContentType = "application/pdf",
                                    ContentLength = new FileInfo(completedFilePath).Length
                                };
                                item.Files.Add(completedMedia);
                                _mediaRepo.ChangeEntityState(completedMedia, EntityState.Added);
                                item.Status = JobStatus.Completed;
                                item.ResponseSignatureApiDate = DateTime.Now;
                                _logger.LogWarning($"CallHashInProgress Sign is completed: jobid={item.Id}");

                            }
                            else
                            {
                                _logger.LogError($"CallHashInProgress: pending file not found: jobid={item.Id}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        item.Status = JobStatus.Failed;
                        item.Note = $"An error occurred in Progress Job: {ex.Message}";
                        _logger.LogError($"CallHashInProgress Error: " + ex);
                    }
                    finally
                    {
                        if (item.Status == JobStatus.Completed)
                        {
                            item.CallBackStatus = CallBackStatus.Pending;
                        }
                        bool isPub = await _publisher.PublishMessage(item.Id.ToString(), _rabbitMQSettings.CallBackJobQueueName, (int)item.Priority);
                        if (isPub)
                        {
                            item.SentToMessageBroker |= 2; // bit 2 set to 1: đã callback
                        }

                        _jobRepo.ChangeEntityState(item, EntityState.Modified);
                        await _uow.SaveChangesAsync();
                        
                        _logger.LogWarning($"CallHashInProgress Job is completed: {item.Id}");
                    }
                }
                else
                {
                    _logger.LogError($"CallHashInProgress: apiSourceData not found: jobid={item.Id}");
                }
                return true;
            }



        }
        private async Task<bool> ProcessPendingFile(Job item)
        {
            bool res = true;
            try
            {
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
                        await _mediaRepo.AddAsync(mediaPendingItem);
                    }
                    else
                    {
                        var mediaPendingItem = ConvertFileOfficeToPdf(item.AppTokenKey, originalItem, item.FilePassword);
                        item.Files.Add(mediaPendingItem);
                        await _mediaRepo.AddAsync(mediaPendingItem);
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
                    await _mediaRepo.AddAsync(mediaPendingItem);
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
            }
            catch (Exception ex)
            {
                item.Status = JobStatus.Failed;
                item.Note = $"An error occurred in Pending Job: {ex.Message}";
                _logger.LogError($"PendingJob: " + ex);
                res = false;
            }
            finally
            {
                res = true;
            }
            return res;
        }

        private Media ConvertFileOfficeToPdf(string appTokenKey, Media originalFile, string password)
        {
            var s = _apiSource.Sources.SingleOrDefault(q => q.Key == appTokenKey);
            var fileId = Guid.NewGuid();
            var fileName = fileId + ".pdf";
            var pendingPath = Path.Combine(s.Folder, Path.GetDirectoryName(originalFile.Path));
            var filePath = Path.Combine(s.Folder, pendingPath, fileName);
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
                doc.Save(filePath, Aspose.Words.SaveFormat.Pdf);
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
                Path = Path.Combine(Path.GetDirectoryName(originalFile.Path), fileName),
                ContentType = "application/pdf",
                ContentLength = new FileInfo(filePath).Length
            };
            return media;
        }
    }
}
