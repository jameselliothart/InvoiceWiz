using Contracts;
using MassTransit;
using MongoDB.Driver;

namespace Persister.Invoices;

public class InvoiceRequestedConsumer(IMongoCollection<Invoice> _invoices, ILogger<InvoiceRequestedConsumer> _logger) : IConsumer<InvoiceRequestedEvent>
{
    public async Task Consume(ConsumeContext<InvoiceRequestedEvent> context)
    {
        var requestedInvoice = context.Message;
        using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["invoiceId"] = requestedInvoice.Id,
                ["requestedInvoice"] = requestedInvoice,
            }
        ))
        {
            _logger.LogInformation("Persisting");
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
            _logger.LogInformation("Persisted");
        }
    }
}

public class InvoiceGeneratedConsumer(IMongoCollection<Invoice> _invoices, ILogger<InvoiceGeneratedConsumer> _logger) : IConsumer<InvoiceGeneratedEvent>
{
    public async Task Consume(ConsumeContext<InvoiceGeneratedEvent> context)
    {
        var generatedInvoice = context.Message;
        using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["invoiceId"] = generatedInvoice.Id,
                ["requestedInvoice"] = generatedInvoice,
            }
        ))
        {
            _logger.LogInformation("Persisting");
            var update = Builders<Invoice>.Update
                .SetOnInsert(i => i.Id, generatedInvoice.Id)
                .Set(i => i.Location, generatedInvoice.Location)
            ;

            await _invoices.UpdateOneAsync(
                i => i.Id == generatedInvoice.Id,
                update,
                new UpdateOptions { IsUpsert = true }
            );
            _logger.LogInformation("Persisted");
        }
    }
}