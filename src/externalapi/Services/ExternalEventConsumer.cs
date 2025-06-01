using MassTransit;
using Microsoft.Extensions.Logging;
using ExternalApi.Contracts;

/// <summary>
/// Consumer class for handling messages from the 'long_lived_service_events' queue.
/// </summary>
public class ExternalEventConsumer : IConsumer<MyServiceEvent>
{
    private readonly ILogger<ExternalEventConsumer> _logger;

    public ExternalEventConsumer(ILogger<ExternalEventConsumer> logger)
    {
        //Console.WriteLine("ExternalEventConsumer initialized.");
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MyServiceEvent> context)
    {
        Console.WriteLine("ExternalEventConsumer Consume method called.");
        _logger.LogInformation("[ExternalEventConsumer] Event Received from long_lived_service_events");
        await Task.CompletedTask;
    }

    public async Task Skip(ConsumeContext<MyServiceEvent> context)
    {
        Console.WriteLine("ExternalEventConsumer Skip method called.");
        _logger.LogWarning("[ExternalEventConsumer] Event Skipped from long_lived_service_events");
        await Task.CompletedTask;
    }
}

// A simple class to represent our event data
public record MyServiceEvent
{
    public required string EventId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public required string Message { get; init; }
}
