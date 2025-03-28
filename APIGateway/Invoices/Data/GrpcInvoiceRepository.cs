using Contracts;
using Grpc.Core;
using Search.Grpc;

namespace APIGateway.Invoices.Data;

public class GrpcInvoiceRepository(InvoiceSearchService.InvoiceSearchServiceClient _client, ILogger<GrpcInvoiceRepository> _logger) : IInvoiceRepository
{
    public async Task<List<InvoiceOverviewDto>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving");
            var request = new GetInvoicesRequest() { };
            var reply = await _client.GetInvoicesAsync(request);
            _logger.LogInformation("Retrieved");
            _logger.LogInformation("Deserializing");
            try
            {
                var invoices = reply.Invoices.Select(i => new InvoiceOverviewDto(
                    Guid.Parse(i.Id),
                    i.To,
                    decimal.Parse(i.Amount),
                    new Uri(i.Url),
                    DateTimeOffset.Parse(i.CreatedDate),
                    DateOnly.Parse(i.InvoiceDate)
                )).ToList();
                _logger.LogInformation("Deserialized {invoiceCount} invoices", invoices.Count);
                return invoices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse {grpcReply}", reply);
                return [];
            }
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed with {rpcStatus}", ex.Status);
            return [];
        }
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { ["invoiceId"] = id }))
        {
            try
            {
                _logger.LogInformation("Retrieving");
                var request = new GetInvoiceRequest() { Id = id.ToString() };
                var reply = await _client.GetInvoiceAsync(request);
                _logger.LogInformation("Retrieved");
                var overview = reply.Overview;
                _logger.LogInformation("Deserializing {grpcReply}", reply);
                try
                {
                    var invoice = new Invoice(
                        Guid.Parse(overview.Id),
                        overview.To,
                        decimal.Parse(overview.Amount),
                        new Uri(overview.Url),
                        DateTimeOffset.Parse(overview.CreatedDate),
                        reply.Details,
                        DateOnly.Parse(overview.InvoiceDate)
                    );
                    _logger.LogInformation("Deserialized to {invoice}", invoice);
                    return invoice;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse {grpcReply}", reply);
                    return null;
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed with {rpcStatus}", ex.Status);
                return null;
            }
        }
    }
}