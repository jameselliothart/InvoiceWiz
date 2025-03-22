using APIGateway.Invoices;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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
    logger.LogInformation("Published event {}", invoice);
    return Results.Accepted();
})
.WithOpenApi();

app.MapGet(INVOICE_PATH, async (IMongoDatabase _database, ILogger<Program> _logger) =>
{
    _logger.LogInformation("Retrieving all invoices");
    var invoiceCollection = _database.GetCollection<Invoice>("invoices");
    var invoices = await invoiceCollection.AsQueryable().ToListAsync();
    _logger.LogInformation("Found {} invoices", invoices.Count);
    return invoices;
})
.WithOpenApi();

app.MapGet(INVOICE_PATH + "/{id}", async (Guid id, IMongoDatabase _database, ILogger<Program> _logger) =>
{
    _logger.LogInformation("Retrieving invoice with id {}", id);
    var invoiceCollection = _database.GetCollection<Invoice>("invoices");
    var invoice = await invoiceCollection.AsQueryable().Where(i => i.Id == id).FirstOrDefaultAsync();
    if (invoice == null)
    {
        _logger.LogInformation("Cannot find invoice with id {}", id);
        return Results.NotFound();
    }
    return Results.Ok(invoice);
})
.WithOpenApi();

app.MapGet(INVOICE_PATH + "/{url}/download",
    async (Uri url, IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
{
    var httpClient = httpClientFactory.CreateClient();
    logger.LogInformation("Downloading {}", url);
    var response = await httpClient.GetAsync(url);

    if (!response.IsSuccessStatusCode)
    {
        logger.LogWarning("Not found {}", url);
        return Results.NotFound();
    }

    var stream = await response.Content.ReadAsStreamAsync();
    var fileName = Path.GetFileName(url.ToString());
    logger.LogInformation("Returning file {}", fileName);
    return Results.File(stream, "application/pdf", fileName);
});

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

app.MapHub<InvoiceHub>("/invoiceHub");
app.Run();