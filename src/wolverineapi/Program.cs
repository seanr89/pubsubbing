using System.Data;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Host.UseWolverine(options =>
{
    options.UseRabbitMq(new Uri("amqp://guest:guest@localhost:5672/"))
        .AutoProvision();
    // Turn on listener connection only in case if you only need to listen for messages
    // The sender connection won't be activated in this case
    //.UseListenerConnectionOnly();

    // Set up a listener for a queue, but also
    // fine-tune the queue characteristics if Wolverine
    // will be governing the queue setup
    options.ListenToRabbitQueue("long_lived_service_events", q =>
    {
        //q.PurgeOnStartup = true;
    }).DeadLetterQueueing(new DeadLetterQueue("incoming-errors"))
    .DefaultIncomingMessage<AppServiceEvent>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //app.MapScalarApiReference();
}

//app.UseHttpsRedirection();


app.MapGet("/", () => "Hello from the External API!");

app.Run();
