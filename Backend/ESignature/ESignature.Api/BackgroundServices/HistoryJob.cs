using AutoMapper;
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.Api.BackgroundServices
{
    public class HistoryJob : BackgroundService
    {
        private readonly ILogger<HistoryJob> _logger;
        private readonly int _maxDay;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ApiSourceData _apiSource;

        public HistoryJob(IServiceScopeFactory serviceScopeFactory, IOptions<ESignatureSetting> options, ApiSourceData apiSource, ILogger<HistoryJob> logger)
        {
            _logger = logger;
            _apiSource = apiSource;
            _maxDay = options.Value.MaxDays;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => Start(stoppingToken));
            return Task.CompletedTask;
        }

        private async Task Start(CancellationToken stoppingToken)
        {
            _logger.LogWarning("History Job started...");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_apiSource.Sources != null)
                    {
                        await ProcessData();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"HistoryJob: " + ex);
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }
        }

        private async Task ProcessData()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var now = DateTime.Now.Date;
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var jobRepo = unitOfWork.GetRepository<Job>();
                var jobHistoryRepo = unitOfWork.GetRepository<JobHistory>();
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                var items = await jobRepo.Query(q => q.CallBackStatus == CallBackStatus.Completed, q => q.Include(k => k.Files), false)
                                         .Where(q => EF.Functions.DateDiffDay(q.CreatedDate.Date, now) >= _maxDay)
                                         .OrderBy(q => q.CreatedDate)
                                         .Take(50)
                                         .ToListAsync();
                if (items.Any())
                {
                    foreach (var item in items)
                    {
                        var history = mapper.Map<JobHistory>(item);
                        foreach (var f in item.Files)
                        {
                            f.JobId = null;
                            f.JobHistoryId = history.Id;
                        }
                        await jobHistoryRepo.AddAsync(history);
                    }
                    jobRepo.RemoveRange(items);
                    await unitOfWork.SaveChangesAsync();
                    _logger.LogWarning($"History Job is completed.");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}