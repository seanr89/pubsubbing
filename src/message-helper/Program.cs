using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events; // Not strictly needed for producer, but good to know it exists

namespace RabbitMqEventSender
{
    public class Program
    {
        // --- RabbitMQ Configuration ---
        private const string RabbitMqHostName = "localhost";
        private const int RabbitMqPort = 5672;
        private const string RabbitMqUserName = "guest"; // Default user for RabbitMQ
        private const string RabbitMqPassword = "guest"; // Default password for RabbitMQ
        private const string QueueName = "long_lived_service_events";

        // --- Service Configuration ---
        private static readonly TimeSpan SendInterval = TimeSpan.FromSeconds(5); // Send an event every 5 seconds

        public static async Task Main(string[] args)
        {
            Console.WriteLine("RabbitMQ Event Sender Service starting...");

            // Setup a CancellationTokenSource for graceful shutdown
            var cts = new CancellationTokenSource();

            // Handle Ctrl+C or process termination signals
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent the process from terminating immediately
                cts.Cancel();    // Signal cancellation
                Console.WriteLine("\nCancellation requested. Shutting down gracefully...");
            };

            try
            {
                // Create a connection factory
                var factory = new ConnectionFactory()
                {
                    HostName = RabbitMqHostName,
                    Port = RabbitMqPort,
                    UserName = RabbitMqUserName,
                    Password = RabbitMqPassword,
                    //DispatchConsumersAsync = true // For async consumers, not strictly needed for producer
                };

                // Establish a connection and create a channel
                // using statement ensures proper disposal of connection and channel
                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                // Declare the queue
                // This is idempotent; the queue will be created only if it doesn't exist
                await channel.QueueDeclareAsync(queue: QueueName,
                                     durable: false,    // Queue survives broker restarts (set to true for persistent messages)
                                     exclusive: false,  // Used by only one connection and deleted when that connection closes
                                     autoDelete: false, // Queue is deleted when last consumer unsubscribes
                                     arguments: null);

                Console.WriteLine($"Connected to RabbitMQ. Sending events to queue: '{QueueName}' every {SendInterval.TotalSeconds} seconds.");
                Console.WriteLine("Press Ctrl+C to stop the service.");

                long eventCounter = 0;

                // Main loop for sending events
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Create an event message
                        var eventMessage = new MyServiceEvent
                        {
                            EventId = Guid.NewGuid().ToString(),
                            Timestamp = DateTimeOffset.UtcNow,
                            Message = $"Hello from the long-lived service! (Event #{++eventCounter})"
                        };

                        // Serialize the event message to JSON
                        string jsonMessage = JsonSerializer.Serialize(eventMessage);
                        var body = Encoding.UTF8.GetBytes(jsonMessage);

                        var props = new BasicProperties();
                        props.ContentType = "text/plain";
                        props.DeliveryMode = DeliveryModes.Persistent;
                        props.Expiration = "36000000";

                        // Publish the message to the queue
                        await channel.BasicPublishAsync(
                            "", // Use the default direct exchange
                            QueueName,
                            false,
                            props,
                            body
                        );

                        Console.WriteLine($" [x] Sent event: '{eventMessage.EventId}' at {eventMessage.Timestamp.ToLocalTime()}");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error sending message: {ex.Message}");
                        // In a real application, you might want to implement retry logic,
                        // circuit breaker patterns, or notify monitoring systems here.
                    }

                    // Wait for the specified interval, respecting cancellation
                    try
                    {
                        await Task.Delay(SendInterval, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // This exception is expected when cts.Cancel() is called during Task.Delay
                        break; // Exit the loop
                    }
                }
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                Console.Error.WriteLine($"RabbitMQ broker is unreachable: {ex.Message}");
                Console.Error.WriteLine("Please ensure RabbitMQ is running and accessible at the configured host/port.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unhandled error occurred: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("RabbitMQ Event Sender Service stopped.");
            }
        }
    }

    // A simple class to represent our event data
    public class MyServiceEvent
    {
        public required string EventId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public required string Message { get; set; }
    }
}