using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using MiddleTier;

var AllowDevAccessPolicy = "allowDevAccessPolicy";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowDevAccessPolicy, policy => policy.AllowAnyOrigin().AllowAnyHeader());
});
builder.Services.AddSingleton((_) =>
{
    var topic = "invoices";
    var config = new ProducerBuilder<Guid, InvoiceRequested>(
        new ProducerConfig
        {
            BootstrapServers = "broker:29092",
            ClientId = "InvoiceWizProducer", // Helps with debugging and monitoring
            MessageTimeoutMs = 5000, // Timeout for message delivery (in milliseconds)
            Acks = Acks.All
        }
    ).SetKeySerializer(new GuidSerializer())
    .SetValueSerializer(new InvoiceRequestedSerializer())
    .Build();
    return new Publisher(topic, config);
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
app.Run();

class Publisher(string topic, IProducer<Guid, InvoiceRequested> producer)
{
    public async Task Publish(InvoiceRequested invoiceDetails)
    {
        var msg = new Message<Guid, InvoiceRequested> { Key = invoiceDetails.Id, Value = invoiceDetails };
        try
        {
            var deliveryResult = await producer.ProduceAsync(topic, msg);
            Console.WriteLine($"{topic}: published {deliveryResult.Key} to partition {deliveryResult.TopicPartitionOffset}");
        }
        catch (ProduceException<Guid, InvoiceRequested> ex)
        {
            Console.WriteLine($"Failed to deliver message: {ex.Error.Reason}");
        }
    }
}