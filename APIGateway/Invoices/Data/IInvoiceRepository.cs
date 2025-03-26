using Contracts;

namespace APIGateway.Invoices.Data;

public interface IInvoiceRepository
{
    Task<List<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<Uri?> GetUrlByIdAsync(Guid id);
}