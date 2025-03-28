using Azure.Storage.Blobs;
using Generator.FileGeneration;
using Generator.Invoices;
using Generator.Storage;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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
builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    var jaegerHost = builder.Configuration["Jaeger:Host"];
    tracerProviderBuilder
        .AddSource("MassTransit") // MassTransit tracing
        .AddHttpClientInstrumentation() // Azurite
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Generator"));

    if (!string.IsNullOrEmpty(jaegerHost))
    {
        tracerProviderBuilder.AddJaegerExporter(o =>
        {
            o.AgentHost = jaegerHost;
            o.AgentPort = 6831;
        });
    }
    else
    {
        Console.WriteLine("Falling back to console trace exporter.");
        tracerProviderBuilder.AddConsoleExporter();
    }
});
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
