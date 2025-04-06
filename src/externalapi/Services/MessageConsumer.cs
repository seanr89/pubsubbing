using ExternalApi.Contracts;
using MassTransit;

public class MyMessageConsumer : IConsumer<MyMessage>
{
    private readonly ILogger<MyMessageConsumer> _logger;

    public MyMessageConsumer(ILogger<MyMessageConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MyMessage> context)
    {
        _logger.LogInformation("Received message: {Text} at {Timestamp}", context.Message.Text, context.Message.Timestamp);
        await Task.CompletedTask;
    }
}