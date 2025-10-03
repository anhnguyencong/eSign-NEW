using ESignature.Core.Helpers;
using ESignature.Core.Infrastructure;
using ESignature.Core.RestClient;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.HashServiceLayer.Services.OnStartup;
using ESignature.HashServiceLayer.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESignature.HashServiceLayer.Services.Commands
{
    public class DoCallBackCommand : IRequest<bool>
    {
        public Guid JobId { get; set; }
    }
    public class DoCallBackCommandHandler : IRequestHandler<DoCallBackCommand, bool>
    {
        private readonly ILogger<DoCallBackCommandHandler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly string _hostUrl;
        private readonly IUnitOfWork _uow;
        private readonly IRepository<Job> _jobRepo;
        private readonly IRestClient _restClient;

        public DoCallBackCommandHandler(ILogger<DoCallBackCommandHandler> logger
            , IServiceScopeFactory serviceScopeFactory, ApiSourceData apiSource
            , IRestClient restClient
            , IOptions<ESignatureSetting> esoptions
            , IUnitOfWork uow)
        {
            _logger = logger;
            _hostUrl = esoptions.Value.HostUrl;
            _restClient = restClient;
            _serviceScopeFactory = serviceScopeFactory;
            _uow = uow;
            
            _jobRepo = _uow.GetRepository<Job>();

        }

        public async Task<bool> Handle(DoCallBackCommand request, CancellationToken cancellationToken)
        {
            bool res = false;
            var item = await _jobRepo.FirstOrDefaultAsync(q => q.Id == request.JobId && q.CallBackStatus == CallBackStatus.Pending,
                                                 q => q.Include(t => t.Files));


            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                await CallBackClient(item);
                item.CallBackStatus = CallBackStatus.Completed;
                item.SentToMessageBroker |= 2; // bit 2 set to 1: đã callback
                stopwatch.Stop();
                _logger.LogWarning($"CallBackJob_ProcessData_CallBackClient: {stopwatch.ElapsedMilliseconds} ms");
                _logger.LogWarning($"Callback Job is completed.");
                res = true;
            }
            catch (Exception ex)
            {
                item.CallBackStatus = CallBackStatus.Failed;
                item.Note = $"Cannot callback to client={item.CallBackUrl} Error:{ex.Message}";
                _logger.LogError($"Callback Job: id={item.Id} ex={ex}");
                res = false;
            }
            finally
            {
                _jobRepo.ChangeEntityState(item, EntityState.Modified);
            }
            await _uow.SaveChangesAsync();
            return res;
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
                _logger.LogWarning($"Called back client:id={item.Id}.url={item.CallBackUrl} ");                
            }
            else
            {
                _logger.LogError($"Called back client cannot be found URL: jobid={item.Id}");
            }
        }
    }
}
