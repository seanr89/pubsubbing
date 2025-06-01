using ExternalApi.Contracts;
using MassTransit;

/// <summary>
/// Consumer class for handling messages of type MyEvent.
/// This class implements the IConsumer interface from MassTransit.
/// It is responsible for processing messages received from the message bus.
/// </summary>
public class EventConsumer : IConsumer<MyEvent>
{
    private readonly ILogger<EventConsumer> _logger;

    public EventConsumer(ILogger<EventConsumer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handle the incoming message.
    /// This method is called when a message of type MyMessage is received.
    /// It logs the message content and timestamp.
    /// The method is asynchronous and returns a Task.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Consume(ConsumeContext<MyEvent> context)
    {
        _logger.LogInformation("Event Received: {Text} for {UserName} at {Timestamp}", context.Message.Content, context.Message.UserName, context.Message.Timestamp);
        await Task.CompletedTask;
    }

}