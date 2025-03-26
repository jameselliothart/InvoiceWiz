using Contracts;
using Grpc.Core;
using Search.Grpc;

namespace APIGateway.Invoices.Data;

public class GrpcInvoiceRepository(InvoiceSearchService.InvoiceSearchServiceClient _client, ILogger<GrpcInvoiceRepository> _logger) : IInvoiceRepository
{
    public Task<List<Invoice>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Invoice?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<Uri?> GetUrlByIdAsync(Guid id)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { ["invoiceId"] = id }))
        {
            _logger.LogInformation("Retrieving");
            try
            {
                var request = new GetInvoiceUrlRequest() { Id = id.ToString() };
                var reply = await _client.GetInvoiceUrlAsync(request);
                _logger.LogInformation("Retrieved");
                return new Uri(reply.Url);
            }
            catch (RpcException e)
            {
                _logger.LogError(e, "Failed with {rpcStatus}", e.Status);
                return null;
            }
        }
    }
}