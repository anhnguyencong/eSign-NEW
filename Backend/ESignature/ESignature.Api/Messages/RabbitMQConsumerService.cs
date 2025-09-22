using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESignature.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ESignature.Api.Messages
{

    public class RabbitMQConsumerPendingService : IHostedService, IDisposable
    {
        private readonly ILogger<RabbitMQConsumerPendingService> _logger;
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IChannel _channel;
        private readonly IConfiguration _config;
        private RabbitMQSettings _rabbitMQSettings;

        public RabbitMQConsumerPendingService(ILogger<RabbitMQConsumerPendingService> logger)
        {
            _logger = logger;
            _rabbitMQSettings = _config.GetSection("LogRabbitMQSettings").Get<RabbitMQSettings>() ?? new RabbitMQSettings();
            _connectionFactory = new ConnectionFactory
            {
                HostName = _rabbitMQSettings.HostName,
                UserName = _rabbitMQSettings.UserName,
                Password = _rabbitMQSettings.Password,
                ConsumerDispatchConcurrency = (ushort)_rabbitMQSettings.ConsumerDispatchConcurrency
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"RabbitMQ consumer service: '{_rabbitMQSettings.PendingJobQueueName}' starting.");

            try
            {
                // Create connection and channel
                _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
                _channel = await _connection.CreateChannelAsync();

                // Declare queue
                await _channel.QueueDeclareAsync(
                    queue: _rabbitMQSettings.PendingJobQueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken
                );

                // Set QoS to process one message at a time
                await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: (ushort)_rabbitMQSettings.PrefetchBasicQos, global: false, cancellationToken: cancellationToken);

                _logger.LogInformation(" [*] Waiting for messages.");

                // Set up consumer
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        byte[] body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation($" [x] Received {message}");

                        // Simulate work based on number of dots
                        int dots = message.Split('.').Length - 1;
                        await Task.Delay(dots * 1000, cancellationToken);

                        _logger.LogInformation(" [x] Done");

                        // Acknowledge the message
                        await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing message: '{_rabbitMQSettings.PendingJobQueueName}'.");
                        // Optionally reject and requeue
                        await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
                    }
                };

                // Start consuming
                await _channel.BasicConsumeAsync(
                    queue: _rabbitMQSettings.PendingJobQueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting RabbitMQ consumer '{_rabbitMQSettings.PendingJobQueueName}' service.");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"RabbitMQ consumer '{_rabbitMQSettings.PendingJobQueueName}' service stopping.");

            try
            {
                // Close channel and connection gracefully
                if (_channel?.IsOpen == true)
                {
                    await _channel.CloseAsync(cancellationToken);
                }
                if (_connection?.IsOpen == true)
                {
                    await _connection.CloseAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error stopping RabbitMQ consumer '{_rabbitMQSettings.PendingJobQueueName}' service.");
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ resources.");
            }
        }
    }

}
