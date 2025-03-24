using Azure.Storage.Blobs;
using Generator.FileGeneration;
using Generator.Invoices;
using Generator.Storage;
using MassTransit;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddSingleton(sp =>
{
    var config = builder.Configuration;
    var blobClient = new BlobServiceClient(config["AzureBlobStorage:ConnectionString"]);
    var containerClient = blobClient.GetBlobContainerClient(config["AzureBlobStorage:ContainerName"]);
    containerClient.CreateIfNotExists();
    return containerClient;
});
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
builder.Services.AddSingleton<IFileGenerator, PdfGenerator>();
builder.Services.AddMassTransit(c =>
{
    var config = builder.Configuration;
    c.AddConsumer<InvoiceRequestedConsumer>();
    c.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(config["RabbitMQ:Host"]);
        cfg.ReceiveEndpoint("generator-queue", e =>
        {
            e.ConfigureConsumers(ctx);
        });
    });
});

var host = builder.Build();
host.Run();
