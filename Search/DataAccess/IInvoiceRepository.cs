using Contracts;

namespace Search.DataAccess;

public interface IInvoiceRepository
{
    Task<List<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(Guid id);
}