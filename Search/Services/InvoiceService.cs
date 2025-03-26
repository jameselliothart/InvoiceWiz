using Grpc.Core;
using Search.DataAccess;
using Search.Grpc;

namespace Search.Services;

public class InvoiceService(IInvoiceRepository _repo, ILogger<InvoiceService> _logger) : InvoiceSearchService.InvoiceSearchServiceBase
{
    public override async Task<GetInvoiceUrlReply> GetInvoiceUrl(GetInvoiceUrlRequest request, ServerCallContext context)
    {
        using (_logger.BeginScope(new Dictionary<string, object>{["invoiceId"] = request.Id}))
        {
            if (!Guid.TryParse(request.Id, out var invoiceId))
            {
                _logger.LogError("Invalid invoice ID format");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid invoice ID format"));
            }
            var invoice = await _repo.GetByIdAsync(invoiceId);
            if (invoice == null)
            {
                _logger.LogWarning("NotFound");
                throw new RpcException(new Status(StatusCode.NotFound, $"Cannot find id {invoiceId}"));
            }
            if (string.IsNullOrEmpty(invoice.Location.ToString()))
            {
                _logger.LogError("Missing invoice url");
                throw new RpcException(new Status(StatusCode.NotFound, $"Missing invoice url for {invoiceId}"));
            }
            return new GetInvoiceUrlReply
            {
                Id = request.Id,
                Url = invoice.Location.ToString(),
            };
        }
    }
}
