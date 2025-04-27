using MassTransit;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddTransient<QueueSenderService>();

builder.Services.AddMassTransit(x =>
{
    // elided...
    x.UsingRabbitMq((context,cfg) =>
    {
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
    //Consumers basically handle Topics via the bus and object type!
    x.AddConsumer<MyMessageConsumer>();
    x.AddConsumer<EventConsumer>();
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
