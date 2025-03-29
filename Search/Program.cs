using Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Search.DataAccess;
using Search.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddGrpc();
// Register MongoDB
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("MongoDb");
    var client = new MongoClient(connStr);
    return client.GetDatabase("InvoiceDb").GetCollection<Invoice>("invoices");
});
builder.Services.AddSingleton<IInvoiceRepository, MongoInvoiceRepository>();
builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    var jaegerHost = builder.Configuration["Jaeger:Host"];
    tracerProviderBuilder
        .AddAspNetCoreInstrumentation() // gRPC server
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Search"))
        .AddConsoleExporter();

    if (!string.IsNullOrEmpty(jaegerHost))
    {
        Console.WriteLine("Adding Jaeger OtlpExporter");
        tracerProviderBuilder.AddOtlpExporter(o => { o.Endpoint = new Uri(jaegerHost); o.Protocol = OtlpExportProtocol.Grpc; });
    }
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<InvoiceService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

app.Run();
