
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

    public async Task<bool> SendMessage(string message)
    {
        // Logic to send a message to the queue
        Console.WriteLine($"Message sent: {message}");
        var myMessage = new MyMessage(message, DateTime.UtcNow);
        await _bus.Publish(myMessage);
        _logger.LogInformation("Published message: {Text}", myMessage.Text);
        return true;
    }
}