using Contracts;
using MassTransit;
using MongoDB.Driver;

namespace Persister.Invoices;

public class InvoiceRequestedConsumer(IMongoDatabase _database, ILogger<InvoiceRequestedConsumer> _logger) : IConsumer<InvoiceRequestedEvent>
{
    private readonly IMongoCollection<Invoice> _invoices = _database.GetCollection<Invoice>("invoices");

    public async Task Consume(ConsumeContext<InvoiceRequestedEvent> context)
    {
        var requestedInvoice = context.Message;
        _logger.LogInformation("Persister received message: {}", requestedInvoice);
        var update = Builders<Invoice>.Update
            .SetOnInsert(i => i.Id, requestedInvoice.Id)
            .Set(i => i.To, requestedInvoice.To)
            .Set(i => i.Amount, requestedInvoice.Amount)
            .Set(i => i.CreatedDate, requestedInvoice.Date)
            .Set(i => i.InvoiceDate, requestedInvoice.InvoiceDate)
            .Set(i => i.Details, requestedInvoice.Details)
        ;

        await _invoices.UpdateOneAsync(
            i => i.Id == requestedInvoice.Id,
            update,
            new UpdateOptions { IsUpsert = true }
        );
        _logger.LogInformation("Persisted message id {}", requestedInvoice.Id);
    }
}

public class InvoiceGeneratedConsumer(IMongoDatabase _database, ILogger<InvoiceGeneratedConsumer> _logger) : IConsumer<InvoiceGeneratedEvent>
{
    private readonly IMongoCollection<Invoice> _invoices = _database.GetCollection<Invoice>("invoices");

    public async Task Consume(ConsumeContext<InvoiceGeneratedEvent> context)
    {
        var generatedInvoice = context.Message;
        _logger.LogInformation("Persister received message: {}", generatedInvoice);
        var update = Builders<Invoice>.Update
            .SetOnInsert(i => i.Id, generatedInvoice.Id)
            .Set(i => i.Location, generatedInvoice.Location)
        ;

        _logger.LogInformation("Persisting message id {}", generatedInvoice.Id);
        await _invoices.UpdateOneAsync(
            i => i.Id == generatedInvoice.Id,
            update,
            new UpdateOptions { IsUpsert = true }
        );
    }
}