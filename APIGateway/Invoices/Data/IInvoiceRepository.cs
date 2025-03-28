using Contracts;

namespace APIGateway.Invoices.Data;

public interface IInvoiceRepository
{
    Task<List<InvoiceOverviewDto>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(Guid id);
}