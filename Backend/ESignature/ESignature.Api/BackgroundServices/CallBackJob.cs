using ESignature.Core.Helpers;
using ESignature.Core.Infrastructure;
using ESignature.Core.RestClient;
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
    public class CallBackJob : BackgroundService
    {
        private readonly ILogger<CallBackJob> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IRestClient _restClient;
        private readonly string _hostUrl;
        private readonly int _maxThreads;
        private readonly ApiSourceData _apiSource;
        private readonly ServiceData _service;

        public CallBackJob(IServiceScopeFactory serviceScopeFactory, ApiSourceData apiSource,
            IRestClient restClient, IOptions<ESignatureSetting> esoptions,
            ILogger<CallBackJob> logger, ServiceData service)
        {
            _logger = logger;
            _service = service;
            _apiSource = apiSource;
            _restClient = restClient;
            _hostUrl = esoptions.Value.HostUrl;
            _maxThreads = esoptions.Value.MaxThreads;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => Start(stoppingToken));
            return Task.CompletedTask;
        }

        private async Task Start(CancellationToken stoppingToken)
        {
            _logger.LogWarning("Callback Job started...");
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
                        _logger.LogWarning($"CallBackJob_ProcessData: {stopwatch.ElapsedMilliseconds} ms");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"CallBackJob: " + ex);
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
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var jobRepo = unitOfWork.GetRepository<Job>();
                var items = await jobRepo.Query(q => q.CallBackStatus == CallBackStatus.Pending,
                                                     q => q.Include(t => t.Files), false)
                                         .OrderBy(q => q.CreatedDate)
                                         .Take(_maxThreads)
                                         .ToListAsync();
                var tasks = new List<Task>();
                foreach (var item in items)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();

                            await CallBackClient(item);
                            item.CallBackStatus = CallBackStatus.Completed;

                            stopwatch.Stop();
                            _logger.LogWarning($"CallBackJob_ProcessData_CallBackClient: {stopwatch.ElapsedMilliseconds} ms");
                        }
                        catch (Exception ex)
                        {
                            item.CallBackStatus = CallBackStatus.Failed;
                            item.Note = $"Cannot callback to client={item.CallBackUrl} Error:{ex.Message}";
                            _logger.LogError($"Callback Job: id={item.Id} ex={ex}");
                        }
                        finally
                        {
                            jobRepo.ChangeEntityState(item, EntityState.Modified);
                        }
                    }));
                }

                Task t = Task.WhenAll(tasks);
                t.Wait();

                if (tasks.Any())
                {
                    await unitOfWork.SaveChangesAsync();

                    if (t.Status == TaskStatus.RanToCompletion)
                        _logger.LogWarning($"Callback Job is completed.");
                    else if (t.Status == TaskStatus.Faulted)
                        _logger.LogError($"Callback Job is faulted.");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        private async Task CallBackClient(Job item)
        {
            var fileCompleted = item.Files.FirstOrDefault(q => q.JobFileType == JobFileType.Completed);
            if (!item.NeedSign)
            {
                fileCompleted = item.Files.FirstOrDefault(q => q.JobFileType == JobFileType.Pending);
            }
            if (!string.IsNullOrEmpty(item.CallBackUrl))
            {
                var data = new
                {
                    RefId = item.RefId,
                    BatchId = item.BatchId,
                    JsonData = item.JsonData,
                    Status = item.Status.ToString(),
                    FileCompletedUrl = fileCompleted?.Id.ToDownloadUrl(_hostUrl),
                    ErrorMessage = item.Note
                };
                await _restClient.PostAsync(item.CallBackUrl, data);
                _logger.LogWarning($"Called back client: {item.CallBackUrl}");
            }
            else
            {
                _logger.LogError($"Called back client cannot be found URL: jobid={item.Id}");
            }
        }
    }
}