namespace APIGateway.Invoices.Files;

public interface IInvoiceFileRepository
{
    Task<IAzureResult> Get(Uri url);
}