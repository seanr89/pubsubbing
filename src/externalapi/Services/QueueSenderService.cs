
using ExternalApi.Contracts;
using MassTransit;

public class QueueSenderService
{
    private readonly IBus _bus;
    private readonly ILogger<QueueSenderService> _logger;

    public QueueSenderService(IBus bus, ILogger<QueueSenderService> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Support attempts to send a message to the queue.
    /// This method is responsible for publishing messages to the message bus.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<bool> SendMessage(string message)
    {
        // Logic to send a message to the queue
        MyMessage myMessage = new(message, DateTime.UtcNow);
        await _bus.Publish(myMessage);
        _logger.LogInformation("Published message: {Text}", myMessage.Text);
        return true;
    }

    public async Task PushToAnotherQueue(string content, string userName)
    {
        // Logic to send a message to another queue
        MyEvent myMessage = new(content, userName, DateTime.UtcNow);
        await _bus.Publish(myMessage);
        _logger.LogInformation("Published message: {Text}", myMessage.Content);
    }
}