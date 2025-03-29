using APIGateway.Invoices;
using APIGateway.Invoices.Data;
using APIGateway.Invoices.Files;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Serilog;
using Search.Grpc;
using Grpc.Net.Client;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    var jaegerHost = builder.Configuration["Jaeger:Host"];
    tracerProviderBuilder
        .AddAspNetCoreInstrumentation() // REST endpoints
        .AddGrpcClientInstrumentation() // gRPC to InvoiceService
        .AddHttpClientInstrumentation() // PDF downloads
        .AddSource("MassTransit") // MassTransit tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("APIGateway"))
        .AddConsoleExporter();

    if (!string.IsNullOrEmpty(jaegerHost))
    {
        Console.WriteLine("Adding Jaeger OtlpExporter");
        tracerProviderBuilder.AddOtlpExporter(o => { o.Endpoint = new Uri(jaegerHost); o.Protocol = OtlpExportProtocol.Grpc; });
    }
});
builder.Services.AddMassTransit(c =>
{
    var config = builder.Configuration;
    c.AddConsumer<InvoiceGeneratedConsumer>();
    c.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(config["RabbitMQ:Host"]);
        cfg.ReceiveEndpoint("apigateway-queue", e =>
        {
            e.ConfigureConsumers(ctx);
        });
    });
});
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});
builder.Services.AddSingleton<IInvoiceRepository, GrpcInvoiceRepository>();
builder.Services.AddSingleton<IInvoiceFileRepository, AzureInvoiceFileRepository>();
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("SearchService") ??
        throw new ArgumentException("Configure SearchService connection string");
    return new InvoiceSearchService.InvoiceSearchServiceClient(GrpcChannel.ForAddress(connStr));
});

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/uuid", () => NewId.NextSequentialGuid()).WithOpenApi();

var INVOICE_PATH = "/api/invoices";

app.MapPost(INVOICE_PATH, async (
    [FromBody] InvoiceRequestedEvent invoice,
    IPublishEndpoint publishEndpoint,
    ILogger<Program> logger
    ) =>
{
    if (invoice.Id == Guid.Empty || string.IsNullOrEmpty(invoice.Id.ToString()))
        return Results.BadRequest("Id must be a nonempty UUID");
    if (invoice.Date == DateTimeOffset.MinValue)
        invoice.Date = DateTimeOffset.UtcNow;

    using (logger.BeginScope(new Dictionary<string, object>
    {
        ["invoiceId"] = invoice.Id,
        ["InvoiceRequestedEvent"] = invoice,
    }
    ))
    {
        logger.LogInformation("Publishing");
        await publishEndpoint.Publish(invoice);
        logger.LogInformation("Published");
        return Results.Accepted();
    }
})
.WithOpenApi();

app.MapGet(INVOICE_PATH, async (IInvoiceRepository _repo, ILogger<Program> _logger) =>
{
    _logger.LogInformation("Retrieving all invoices");
    var invoices = await _repo.GetAllAsync();
    _logger.LogInformation("Retrieved {invoiceCount} invoices", invoices.Count);
    return invoices;
})
.WithOpenApi();

app.MapGet(INVOICE_PATH + "/{id}", async (Guid id, IInvoiceRepository _repo, ILogger<Program> _logger) =>
{
    using (_logger.BeginScope(new Dictionary<string, object> { ["invoiceId"] = id }))
    {
        _logger.LogInformation("Retrieving");
        var invoice = await _repo.GetByIdAsync(id);
        if (invoice == null)
        {
            _logger.LogInformation("NotFound");
            return Results.NotFound();
        }
        _logger.LogInformation("Retrieved");
        return Results.Ok(invoice);
    }
})
.WithOpenApi();

app.MapGet(INVOICE_PATH + "/{id}/download",
    async (Guid id, IInvoiceFileRepository _fileRepo, ILogger<Program> _logger) =>
{
    using (_logger.BeginScope(new Dictionary<string, object> { ["invoiceId"] = id }))
    {
        _logger.LogInformation("Download start");
        var downloadResult = await _fileRepo.Get(id);
        var result = downloadResult switch
        {
            DownloadFailure f => Results.StatusCode(f.StatusCode),
            DownloadSuccess s => Results.File(s.Stream, "application/pdf", s.FileName),
            _ => throw new ArgumentException(message: $"Unknown result type {downloadResult}")
        };
        _logger.LogInformation("Download end");
        return result;
    }
})
.WithOpenApi();

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

app.MapHub<InvoiceHub>("/invoiceHub");
app.Run();