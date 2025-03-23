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
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient("mongodb://mongodb:27017"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase("InvoiceDb"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoDatabase>().GetCollection<Invoice>("invoices"));

builder.Services.AddMassTransit(c =>
{
    c.AddConsumer<InvoiceRequestedConsumer>();
    c.AddConsumer<InvoiceGeneratedConsumer>();
    c.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("rabbitmq://broker");
        cfg.ReceiveEndpoint("persister-queue", e =>
        {
            e.ConfigureConsumers(ctx);
        });
    });
});

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

var host = builder.Build();
host.Run();
