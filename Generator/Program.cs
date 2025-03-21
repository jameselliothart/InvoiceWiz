using Generator.Invoices;
using Generator.Storage;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<AzureBlobStorageOptions>(builder.Configuration.GetSection("AzureBlobStorage"));
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
builder.Services.AddMassTransit(c =>
{
    c.AddConsumer<InvoiceRequestedConsumer>();
    c.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("rabbitmq://broker");
        cfg.ConfigureEndpoints(ctx);
        cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
    });
});

var host = builder.Build();
host.Run();
