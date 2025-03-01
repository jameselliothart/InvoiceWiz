using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MiddleTier;

var AllowDevAccessPolicy = "allowDevAccessPolicy";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowDevAccessPolicy, policy =>
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});
builder.Services.AddSingleton((_) =>
{
    var topic = "invoices";
    var producer = new ProducerBuilder<string, InvoiceRequested>(
        new ProducerConfig
        {
            BootstrapServers = "broker:29092",
            ClientId = "InvoiceWizProducer", // Helps with debugging and monitoring
            MessageTimeoutMs = 5000, // Timeout for message delivery (in milliseconds)
            Acks = Acks.All
        }
    )
    .SetValueSerializer(new InvoiceRequestedSerializer())
    .Build();
    return new Publisher(topic, producer);
});
builder.Services.AddSingleton(_ =>
{
    var consumer = new ConsumerBuilder<string, string>(
        new ConsumerConfig
        {
            BootstrapServers = "broker:29092",
            GroupId = "group-invoice-mt",
            ClientId = "InvoiceWizConsumer", // Helps with debugging and monitoring
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
        }
    ).Build();
    return consumer;
});
builder.Services.AddHostedService(sp =>
{
    var topic = "invoices-generated";
    var consumer = sp.GetRequiredService<IConsumer<string, string>>();
    var hubContext = sp.GetRequiredService<IHubContext<InvoiceHub>>();
    return new Subscriber(topic, consumer, hubContext);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var invoiceRoute = "/api/invoice";

app.MapPost(invoiceRoute, async ([FromBody] InvoiceRequested invoiceDetails, Publisher publisher) =>
{
    Console.WriteLine($"Received {invoiceDetails}");
    await publisher.Publish(invoiceDetails);

    return Results.Accepted();
});

app.UseCors(AllowDevAccessPolicy);
app.MapHub<InvoiceHub>("/api/live");
app.Run();
