using Contracts;
using Grpc.Core;
using Search.Grpc;

namespace APIGateway.Invoices.Data;

public class GrpcInvoiceRepository(InvoiceSearchService.InvoiceSearchServiceClient _client, ILogger<GrpcInvoiceRepository> _logger) : IInvoiceRepository
{
    public async Task<List<InvoiceSummaryDto>> GetAllAsync()
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
                var invoices = reply.Invoices.Select(i => new InvoiceSummaryDto(
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
                var summary = reply.Summary;
                _logger.LogInformation("Deserializing {grpcReply}", reply);
                try
                {
                    var invoice = new Invoice(
                        Guid.Parse(summary.Id),
                        summary.To,
                        decimal.Parse(summary.Amount),
                        new Uri(summary.Url),
                        DateTimeOffset.Parse(summary.CreatedDate),
                        reply.Details,
                        DateOnly.Parse(summary.InvoiceDate)
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