// Services/RabbitMQConsumerService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace rabbbitapi
{
    public class RabbitMQConsumerService : IHostedService, IDisposable
    {
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private readonly string _hostname = "localhost"; // Replace with your RabbitMQ hostname
        private readonly string _queueName = "long_lived_service_events"; // Replace with your queue name
        private IConnection _connection;
        private IChannel _channel;
        private AsyncEventingBasicConsumer _consumer;

        public RabbitMQConsumerService(ILogger<RabbitMQConsumerService> logger)
        {
            _logger = logger;
            InitializeRabbitMQ();
        }

        async Task InitializeRabbitMQAsync()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = _hostname };
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                await _channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                _consumer = new AsyncEventingBasicConsumer(_channel);
                _consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($" [x] Received message: {message}");

                    // **Process your message here**
                    await ProcessMessageAsync(message);

                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing RabbitMQ: {ex.Message}");
            }
        }

        private void InitializeRabbitMQ()
        {
            Task.Run(() => InitializeRabbitMQAsync()).GetAwaiter().GetResult();
        }

        private async Task ProcessMessageAsync(string message)
        {
            // Implement your message processing logic here
            _logger.LogInformation($"Processing message: {message}");
            // Example: Save to database, call another service, etc.
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitMQ Consumer Service started.");
            await _channel?.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: _consumer);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitMQ Consumer Service stopping.");
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
        }
    }
}