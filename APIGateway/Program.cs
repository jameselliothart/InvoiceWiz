using APIGateway.Invoices;
using APIGateway.Invoices.Data;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.AddMassTransit(c =>
{
    c.AddConsumer<InvoiceGeneratedConsumer>();
    c.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("rabbitmq://broker");
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
// Register MongoDB
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient("mongodb://mongodb:27017"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase("InvoiceDb"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoDatabase>().GetCollection<Invoice>("invoices"));
builder.Services.AddSingleton<IInvoiceRepository, MongoInvoiceRepository>();

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

    await publishEndpoint.Publish(invoice);
    logger.LogInformation("Published {InvoiceRequestedEvent} {invoiceId}", invoice, invoice.Id);
    return Results.Accepted();
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
    using (_logger.BeginScope(new Dictionary<string, object>{["invoiceId"] = id}))
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
    async (Guid id, IHttpClientFactory httpClientFactory, IInvoiceRepository _repo, ILogger<Program> _logger) =>
{
    _logger.LogInformation("Retrieving {invoiceId}", id);
    var invoice = await _repo.GetByIdAsync(id);
    if (invoice == null)
    {
        _logger.LogInformation("NotFound: {invoiceId}", id);
        return Results.NotFound();
    }
    _logger.LogInformation("Retrieved {invoiceId}", id);

    var url = invoice.Location;
    var httpClient = httpClientFactory.CreateClient();
    _logger.LogInformation("Downloading {url} {invoiceId}", url, id);
    // Add SharedKey auth (for Azurite demo only)
    var request = new HttpRequestMessage(HttpMethod.Get, url);
    request.Headers.Add("x-ms-version", "2020-04-08"); // Azurite-compatible version
    request.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("R")); // RFC 1123
    request.Headers.Add("Authorization", GenerateSharedKeyAuth(url, "GET"));

    var response = await httpClient.SendAsync(request);

    if (!response.IsSuccessStatusCode)
    {
        _logger.LogWarning("Failed to download {url}: {statusCode}", url, response.StatusCode);
        return Results.StatusCode((int)response.StatusCode);
    }

    var stream = await response.Content.ReadAsStreamAsync();
    var fileName = Path.GetFileName(url.ToString());
    _logger.LogInformation("Downloaded {url} {invoiceId}", url, id);
    return Results.File(stream, "application/pdf", fileName);
})
.WithOpenApi();

// Simple SharedKey auth (for demo, not production-ready)
static string GenerateSharedKeyAuth(Uri uri, string method)
{
    var accountName = "devstoreaccount1";
    var accountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
    var stringToSign = $"{method}\n\n\n\n\n\n\n\n\n\n\n\nx-ms-date:{DateTime.UtcNow:R}\nx-ms-version:2020-04-08\n/{accountName}{uri.AbsolutePath}";
    using var hmac = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(accountKey));
    var signature = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToSign)));
    return $"SharedKey {accountName}:{signature}";
}

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

app.MapHub<InvoiceHub>("/invoiceHub");
app.Run();