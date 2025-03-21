using APIGateway.Invoices;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddMassTransit(c =>
{
    c.AddConsumer<InvoiceGeneratedConsumer>();
    c.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("rabbitmq://broker");
        cfg.ConfigureEndpoints(ctx);
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/uuid", () => NewId.NextSequentialGuid()).WithOpenApi();

var INVOICE_PATH = "/invoices";

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

app.MapGet(INVOICE_PATH, () =>
{
    return "hello";
})
// .WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet(INVOICE_PATH + "/{id}", (Guid id) =>
{
    return "hello " + id.ToString();
});

app.MapHub<InvoiceHub>($"{INVOICE_PATH}/hub");
app.Run();