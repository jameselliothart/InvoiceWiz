using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
namespace APIGateway.Invoices;

public class InvoiceGeneratedConsumer(
    IHubContext<InvoiceHub> _hubContext,
    ILogger<InvoiceGeneratedConsumer> _logger
    ) : IConsumer<InvoiceGeneratedEvent>
{
    private const string RTMethod = "InvoiceGenerated";
    public async Task Consume(ConsumeContext<InvoiceGeneratedEvent> context)
    {
        var generatedInvoice = context.Message;
        using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["method"] = RTMethod,
                ["invoiceId"] = generatedInvoice.Id,
                ["generatedInvoice"] = generatedInvoice,
            }
        ))
        {
            _logger.LogInformation("Notifying hub clients");
            await _hubContext.Clients.All.SendAsync(RTMethod,
                generatedInvoice.Id, generatedInvoice.Location);
            _logger.LogInformation("Notified hub clients");
        }
    }
}
