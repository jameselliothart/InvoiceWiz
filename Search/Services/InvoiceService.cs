using Contracts;
using Grpc.Core;
using Search.DataAccess;
using Search.Grpc;

namespace Search.Services;

public class InvoiceService(IInvoiceRepository _repo, ILogger<InvoiceService> _logger) : InvoiceSearchService.InvoiceSearchServiceBase
{
    public override async Task<GetInvoicesReply> GetInvoices(GetInvoicesRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Retrieving all invoices");
        var invoices = await _repo.GetAllAsync();
        _logger.LogInformation("Retrieved {invoiceCount} invoices", invoices.Count);
        _logger.LogInformation("Preparing reply");
        var invoiceOverviews = invoices.Select(SerializeToProto).Where(i => i != null).ToList();
        _logger.LogInformation("Reply has {protoMsgCount} invoices", invoiceOverviews.Count);
        if (invoices.Count != invoiceOverviews.Count)
            _logger.LogError(
                "There were errors serializing some invoices: {invoiceCount} vs {protoMsgCount}. Check logs.",
                invoices.Count,
                invoiceOverviews.Count
            );
        var reply = new GetInvoicesReply();
        reply.Invoices.AddRange(invoiceOverviews);
        _logger.LogInformation("Prepared reply");
        return reply;
    }

    public InvoiceOverview? SerializeToProto(Invoice invoice)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { ["invoiceId"] = invoice.Id }))
        {
            try
            {
                _logger.LogInformation("Serializing");
                var id = invoice.Id.ToString();
                var url = invoice.Location.ToString();
                var to = invoice.To;
                var amount = invoice.Amount.ToString("G29");
                var invoiceDate = invoice.InvoiceDate.ToString("yyyy-MM-dd");
                var createdDate = invoice.CreatedDate.ToString("o");
                var overview = new InvoiceOverview
                    {
                        Id = id,
                        Url = url,
                        To = to,
                        Amount = amount,
                        InvoiceDate = invoiceDate,
                        CreatedDate = createdDate,
                    };
                _logger.LogInformation("Serialized");
                return overview;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Serialization failed for {invoice}", invoice);
                return null;
            }
        }
    }

    public override async Task<GetInvoiceReply> GetInvoice(GetInvoiceRequest request, ServerCallContext context)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { ["invoiceId"] = request.Id }))
        {
            // TODO codify errors as oneof message replies
            if (!Guid.TryParse(request.Id, out var invoiceId))
            {
                _logger.LogError("Invalid invoice ID format");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid invoice ID format"));
            }
            _logger.LogInformation("Retrieving");
            var invoice = await _repo.GetByIdAsync(invoiceId);
            if (invoice == null)
            {
                _logger.LogWarning("NotFound");
                throw new RpcException(new Status(StatusCode.NotFound, $"Cannot find id {invoiceId}"));
            }
            _logger.LogInformation("Retrieved");
            return new GetInvoiceReply
            {
                Overview = SerializeToProto(invoice),
                Details = invoice.Details ?? "",
            };
        }
    }
}
