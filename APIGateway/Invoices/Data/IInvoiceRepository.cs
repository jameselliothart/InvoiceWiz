using Contracts;

namespace APIGateway.Invoices.Data;

public interface IInvoiceRepository
{
    Task<List<InvoiceSummaryDto>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(Guid id);
}