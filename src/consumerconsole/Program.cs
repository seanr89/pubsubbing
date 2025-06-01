// Program.cs
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace RabbitMQConsumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("RabbitMQ Message Consumer Application");
            Console.WriteLine("Press [Ctrl+C] to exit.");

            // Establish a connection to the RabbitMQ server.
            // Replace "localhost" with your RabbitMQ server's hostname or IP address if it's not local.
            var factory = new ConnectionFactory() { HostName = "localhost" };

            // Use a try-catch block to handle potential connection errors.
            try
            {
                using (var connection = await factory.CreateConnectionAsync())
                using (var channel = await connection.CreateChannelAsync())
                {
                    // Declare a queue. This is idempotent, meaning it will only be created if it doesn't exist.
                    // The queue name should match the one used by the producer.
                    string queueName = "long_lived_service_events";
                    await channel.QueueDeclareAsync(
                        queue: queueName,
                        durable: true,    // durable: true makes the queue survive a broker restart.
                        exclusive: false,  // exclusive: true makes the queue only accessible by the current connection and deleted when that connection closes.
                        autoDelete: false, // autoDelete: true makes the queue deleted when its last consumer unsubscribes.
                        arguments: null    // arguments: optional arguments for queue declaration (e.g., message TTL, queue length limit).
                    );

                    Console.WriteLine($"Waiting for messages in queue '{queueName}'.");

                    // Create a consumer that will listen for messages.
                    var consumer = new AsyncEventingBasicConsumer(channel);

                    // Attach an event handler to process messages when they are received.
                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        // Get the message body as a byte array.
                        var body = ea.Body.ToArray();

                        // Convert the byte array to a string (assuming UTF-8 encoding).
                        var message = Encoding.UTF8.GetString(body);

                        // Print the received message to the console.
                        Console.WriteLine($" [x] Received: {message}");

                        // Acknowledge the message. This tells RabbitMQ that the message has been successfully processed
                        // and can be removed from the queue. If not acknowledged, RabbitMQ will redeliver the message
                        // to another consumer or back to the queue if no other consumers are available.
                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    };

                    // Start consuming messages from the specified queue.
                    // autoAck: true means messages are automatically acknowledged as soon as they are delivered.
                    // We set it to false here because we want to manually acknowledge after processing.
                    await channel.BasicConsumeAsync(
                        queue: queueName,
                        autoAck: false,
                        consumer: consumer
                    );

                    // Keep the application running indefinitely to continue listening for messages.
                    // A ManualResetEvent is used here to block the main thread until a signal is received
                    // (e.g., Ctrl+C, which triggers the Console.CancelKeyPress event).
                    var exitSignal = new ManualResetEvent(false);
                    Console.CancelKeyPress += (sender, eventArgs) =>
                    {
                        Console.WriteLine("Exiting consumer application...");
                        eventArgs.Cancel = true; // Prevent the process from terminating immediately
                        exitSignal.Set(); // Signal the main thread to exit the waiting state
                    };

                    exitSignal.WaitOne(); // Wait until the exit signal is set
                }
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                Console.WriteLine($"Error: Could not connect to RabbitMQ broker. Please ensure RabbitMQ is running and accessible at '{factory.HostName}'.");
                Console.WriteLine($"Details: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Consumer application has stopped.");
            }
        }
    }
}
