using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Logging;
using ESignature.Core.Helpers;
using ESignature.Core.Settings;
using Microsoft.Extensions.Configuration;

namespace ESignature.Api.Messages
{
    public interface IMessagePublisher
    {
        Task<bool> PublishMessage(string message, string queueName);
    }
    public class MessagePublisher : IMessagePublisher, IDisposable
    {

        private readonly ILogger<MessagePublisher> _logger;
        private IConnection _connection;
        private IChannel _channel;
        private readonly IConfiguration _config;
        private RabbitMQSettings _rabbitMQSettings;
        private bool _disposed;
        private static int retry = 0;
        private static DateTime lastRetry = DateTime.Now;
        private readonly object _lock = new object();

        public MessagePublisher(ILogger<MessagePublisher> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            _rabbitMQSettings = _config.GetSection("LogRabbitMQSettings").Get<RabbitMQSettings>() ?? new RabbitMQSettings();
            Connect();
        }

        public void Connect()
        {
            lock (_lock)
            {
                try
                {
                    if (_connection != null && _connection.IsOpen) return;
                    _connection?.Dispose();
                    _channel?.Dispose();

                    var factory = new ConnectionFactory()
                    {
                        HostName = _rabbitMQSettings.HostName,
                        UserName = _rabbitMQSettings.UserName,
                        Password = _rabbitMQSettings.Password,
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(30)
                    };
                    AsyncHelper.RunSync(
                        async () =>
                        {
                            _connection = await factory.CreateConnectionAsync();
                            _channel = await _connection.CreateChannelAsync();
                        }
                        );


                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to connect RabbitMQ for logging: {ex.Message}");
                    _connection?.Dispose();
                    _connection = null;
                    _channel?.Dispose();
                    _channel = null;
                }
            }
        }
        public async Task<bool> PublishMessage(string message, string queueName)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MessagePublisher));

            if (_connection == null || !_connection.IsOpen)
            {
                await Task.Run(() => Connect()); // Run Connect on a background thread
            }

            if (_connection != null && _connection.IsOpen)
            {
                try
                {
                    await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);
                    var properties = new BasicProperties
                    {
                        Persistent = true
                    };

                    properties.Persistent = true; // Make messages durable
                    await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, mandatory: true, basicProperties: properties, body: body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to publish log RabbitMQ: {ex.Message}", message);
                    return false;
                }
            }
            return true;
        }


        // Clean up resources
        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                try
                {
                    AsyncHelper.RunSync(async () =>
                    {
                        await _channel?.CloseAsync();
                        await _connection?.CloseAsync();
                    });

                    _channel?.Dispose();
                    _connection?.Dispose();
                }
                catch (Exception ex)
                {
                    // Log the exception
                    _logger.LogError(ex, $"Error during disposal: {ex.Message}");
                }

                _disposed = true;
            }
        }
    }

}
