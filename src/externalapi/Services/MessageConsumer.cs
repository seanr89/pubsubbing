using ExternalApi.Contracts;
using MassTransit;

/// <summary>
/// Consumer class for handling messages of type MyMessage.
/// This class implements the IConsumer interface from MassTransit.
/// It is responsible for processing messages received from the message bus.
/// </summary>
public class MyMessageConsumer : IConsumer<MyMessage>
{
    private readonly ILogger<MyMessageConsumer> _logger;

    public MyMessageConsumer(ILogger<MyMessageConsumer> logger)
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
    public async Task Consume(ConsumeContext<MyMessage> context)
    {
        _logger.LogInformation("MyMessage Received: {Text} at {Timestamp}", context.Message.Text, context.Message.Timestamp);
        await Task.CompletedTask;
    }

    
}