namespace APIGateway.Invoices.Files;

public interface IInvoiceFileRepository
{
    Task<IAzureResult> Get(Guid id);
}