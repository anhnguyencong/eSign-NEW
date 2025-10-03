using ESignature.Core.Helpers;
using ESignature.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace ESignature.HashServiceLayer.Messages
{
    public interface IMessagePublisher
    {
        Task<bool> PublishMessage(string message, string queueName, int Priority, CancellationToken cancellation = default);
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
            AsyncHelper.RunSync(
                          async () =>
                          {
                              await Connect();
                          });

        }

        public async Task Connect(CancellationToken cancellation = default)
        {

            try
            {
                lock (_lock)
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
        // Publish a message to the specified queue with priority handling
        // Priority: 1 (high), 5 (medium), 10 (low) => but when change to RabbitMQ, it will be 10 (high), 5 (medium), 1 (low)
        public Task<bool> PublishMessage(string message, string queueName)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> PublishMessage(string message, string queueName, int Priority, CancellationToken cancellation = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MessagePublisher));

            if (_connection == null || !_connection.IsOpen)
            {
                await Connect(); // Ensure connection is established       
            }

            if (_connection != null && _connection.IsOpen)
            {
                try
                {
                    // Declare a durable priority queue with max priority 10
                    var arguments = new Dictionary<string, object>
                                            {
                                                { "x-max-priority", 10 }
                                            };
                    await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);

                    var body = Encoding.UTF8.GetBytes(message);
                    var properties = new BasicProperties
                    {
                        Persistent = true, // For durability in task queue
                        //Chuyển đổi priority của Esign qua Priority của RabbitMQ. 2 cái đảo giá trị: 10->1, 1->10, 2->9, 3->8, 4->7, 5->6, 6->5, 7->4, 8->3, 9->2
                        Priority = (byte)(10 - Priority + 1) // RabbitMQ priority => priority này trở thành ưu tiên get khi consumer lấy message
                    };

                    properties.Persistent = true; // Make messages durable
                    await _channel.BasicPublishAsync(exchange: string.Empty
                                                 , routingKey: queueName
                                                 , mandatory: true
                                                 , basicProperties: properties
                                                 , body: body
                                                 , cancellationToken: cancellation);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to publish log RabbitMQ: {ex.Message}", message);
                    return false;
                }
            }
            else
            {
                _logger.LogError($"RabbitMQ connection is not available.");
                return false;
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
