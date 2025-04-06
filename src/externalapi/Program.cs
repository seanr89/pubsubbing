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
    //x.AddConsumer<MyMessageConsumer>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");

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

app.Run();
