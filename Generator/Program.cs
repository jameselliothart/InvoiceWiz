using Generator.FileGeneration;
using Generator.Invoices;
using Generator.Storage;
using MassTransit;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
builder.Services.Configure<AzureBlobStorageOptions>(builder.Configuration.GetSection("AzureBlobStorage"));
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
builder.Services.AddSingleton<IFileGenerator, PdfGenerator>();
builder.Services.AddMassTransit(c =>
{
    c.AddConsumer<InvoiceRequestedConsumer>();
    c.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("rabbitmq://broker");
        cfg.ReceiveEndpoint("generator-queue", e =>
        {
            e.ConfigureConsumers(ctx);
        });
    });
});

var host = builder.Build();
host.Run();
