using Contracts;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Persister.Invoices;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
// Register MongoDB
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("MongoDb");
    var client = new MongoClient(connStr);
    return client.GetDatabase("InvoiceDb").GetCollection<Invoice>("invoices");
});

builder.Services.AddMassTransit(c =>
{
    var config = builder.Configuration;
    c.AddConsumer<InvoiceRequestedConsumer>();
    c.AddConsumer<InvoiceGeneratedConsumer>();
    c.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(config["RabbitMQ:Host"]);
        cfg.ReceiveEndpoint("persister-queue", e =>
        {
            e.ConfigureConsumers(ctx);
        });
    });
});

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

var host = builder.Build();
host.Run();
