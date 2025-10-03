using ESignature.Core.Infrastructure;
using ESignature.Core.Settings;
using ESignature.HashServiceLayer.Services;
using ESignature.HashServiceLayer.Services.Commands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace ESignature.HashServiceLayer.Messages
{

    public class RabbitMQConsumerProgressService : IHostedService, IDisposable
    {
        private readonly ILogger<RabbitMQConsumerProgressService> _logger;
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IChannel _channelInProgress;
        private IChannel _channelCallBack;
        private readonly IConfiguration _config;
        private RabbitMQSettings _rabbitMQSettings;
        private readonly IHashInProgressSignService _inprogressSignService;
        private IMediator _mediator;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMQConsumerProgressService(ILogger<RabbitMQConsumerProgressService> logger
            , IConfiguration config
            , IHashInProgressSignService inprogressSignService
            , IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _config = config;
            _rabbitMQSettings = _config.GetSection("LogRabbitMQSettings").Get<RabbitMQSettings>() ?? new RabbitMQSettings();
            _connectionFactory = new ConnectionFactory
            {
                HostName = _rabbitMQSettings.HostName,
                UserName = _rabbitMQSettings.UserName,
                Password = _rabbitMQSettings.Password,
                ConsumerDispatchConcurrency = (ushort)_rabbitMQSettings.ConsumerDispatchConcurrency
            };
            _inprogressSignService = inprogressSignService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        private async Task WorkingForQueueInProgress(CancellationToken cancellationToken)
        {
            try
            {
                // Create connection and channel
                if (_connection == null || !_connection.IsOpen)
                    _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

                _channelInProgress = await _connection.CreateChannelAsync();

                // Declare the queue (idempotent, matches producer)
                var arguments = new Dictionary<string, object>
                                {
                                    { "x-max-priority", 10 }
                                };

                // Declare queue InProgressJob
                await _channelInProgress.QueueDeclareAsync(
                    queue: _rabbitMQSettings.InProgressJobQueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: arguments,
                    cancellationToken: cancellationToken
                );

                // Set QoS to process one message at a time
                await _channelInProgress.BasicQosAsync(prefetchSize: 0, prefetchCount: (ushort)_rabbitMQSettings.PrefetchBasicQos, global: false, cancellationToken: cancellationToken);

                _logger.LogInformation(" [*] Waiting for messages.");

                // Set up consumer
                var consumer = new AsyncEventingBasicConsumer(_channelInProgress);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        byte[] body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation($" [x] Received {message}");

                        await _inprogressSignService.CallHashInProgress(Guid.Parse(message));

                        _logger.LogInformation(" [x] Done");

                        // Acknowledge the message
                        await _channelInProgress.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing message: '{_rabbitMQSettings.InProgressJobQueueName}'.");
                        // Optionally reject and requeue
                        await _channelInProgress.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
                    }
                };

                // Start consuming
                await _channelInProgress.BasicConsumeAsync(
                    queue: _rabbitMQSettings.InProgressJobQueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting RabbitMQ consumer '{_rabbitMQSettings.InProgressJobQueueName}' service.");
            }
        }

        private async Task WorkingForQueueCallBack(CancellationToken cancellationToken)
        {
            try
            {
                //Create connection and channel
                if (_connection == null || !_connection.IsOpen)
                    _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

                _channelCallBack = await _connection.CreateChannelAsync();

                //Declare the queue(idempotent, matches producer)
                var arguments = new Dictionary<string, object>
                                {
                                    { "x-max-priority", 10 }
                                };

                // Declare queue CallBackJob
                await _channelCallBack.QueueDeclareAsync(
                   queue: _rabbitMQSettings.CallBackJobQueueName,
                   durable: true,
                   exclusive: false,
                   autoDelete: false,
                   arguments: arguments,
                   cancellationToken: cancellationToken
               );

                // Set QoS to process one message at a time
                await _channelCallBack.BasicQosAsync(prefetchSize: 0, prefetchCount: (ushort)_rabbitMQSettings.PrefetchBasicQos, global: false, cancellationToken: cancellationToken);

                _logger.LogInformation(" [*] Waiting for messages.");

                // Set up consumer
                var consumer = new AsyncEventingBasicConsumer(_channelCallBack);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        byte[] body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation($" [x] Received {message}");

                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                            await _mediator.Send(new DoCallBackCommand { JobId = Guid.Parse(message) });
                        }

                        _logger.LogInformation(" [x] Done");

                        //Acknowledge the message
                        await _channelCallBack.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing message: '{_rabbitMQSettings.CallBackJobQueueName}'.");
                        //Optionally reject and requeue
                        await _channelCallBack.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
                    }
                };

                //Start consuming
                await _channelCallBack.BasicConsumeAsync(
                    queue: _rabbitMQSettings.CallBackJobQueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting RabbitMQ consumer '{_rabbitMQSettings.InProgressJobQueueName}' service.");
            }
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"RabbitMQ consumer service: '{_rabbitMQSettings.InProgressJobQueueName}' starting.");

            await WorkingForQueueInProgress(cancellationToken);

            await WorkingForQueueCallBack(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"RabbitMQ consumer '{_rabbitMQSettings.InProgressJobQueueName}' service stopping.");

            try
            {
                // Close channel and connection gracefully
                if (_channelInProgress?.IsOpen == true)
                {
                    await _channelInProgress.CloseAsync(cancellationToken);
                }
                if (_channelCallBack?.IsOpen == true)
                {
                    await _channelCallBack.CloseAsync(cancellationToken);
                }
                if (_connection?.IsOpen == true)
                {
                    await _connection.CloseAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error stopping RabbitMQ consumer '{_rabbitMQSettings.InProgressJobQueueName}' service.");
            }
        }

        public void Dispose()
        {
            try
            {
                _channelInProgress?.Dispose();
                _channelCallBack?.Dispose();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ resources.");
            }
        }
    }

}
