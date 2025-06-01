using MassTransit;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddTransient<QueueSenderService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MyMessageConsumer>();
    x.AddConsumer<EventConsumer>();
    x.AddConsumer<ExternalEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Bind ExternalEventConsumer to the specific queue name
        cfg.ReceiveEndpoint("long_lived_service_events", e =>
        {
            Console.WriteLine("Configuring long_lived_service_events endpoint...");
            e.ConfigureConsumer<ExternalEventConsumer>(context);
            e.DiscardSkippedMessages();
        });

        cfg.ConfigureEndpoints(context);
    });

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

//app.UseHttpsRedirection();

app.MapPost("/sendmessage", async (QueueSenderService sender, string message) =>
{
    var result = await sender.SendMessage(message);
    if (!result)
    {
        return Results.BadRequest("Failed to send message.");
    }
    Console.WriteLine($"Message sent: {message}");
    return Results.Ok(result);
});

app.MapPost("/pushToAnotherQueue", async (QueueSenderService sender, string content, string userName) =>
{
    await sender.PushToAnotherQueue(content, userName);
    return Results.Ok("Message sent to another queue.");
});

app.Run();
