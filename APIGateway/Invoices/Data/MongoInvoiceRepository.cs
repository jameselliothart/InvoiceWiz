using Contracts;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace APIGateway.Invoices.Data;

public class MongoInvoiceRepository(IMongoCollection<Invoice> _collection, ILogger<MongoInvoiceRepository> _logger) : IInvoiceRepository
{
    public async Task<List<Invoice>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all invoices");
        var invoices = await _collection.AsQueryable().ToListAsync();
        _logger.LogInformation("Retrieved {invoiceCount} invoices", invoices.Count);
        return invoices;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving {invoiceId}", id);
        var invoice = await _collection.AsQueryable().Where(i => i.Id == id).FirstOrDefaultAsync();
        if (invoice == null) _logger.LogInformation("NotFound: {invoiceId}", id);
        else _logger.LogInformation("Retrieved {invoiceId}", id);
        return invoice;
    }
}
