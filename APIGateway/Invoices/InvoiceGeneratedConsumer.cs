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
        _logger.LogInformation("Notifying hub clients on {} of {}", RTMethod, generatedInvoice);
        await _hubContext.Clients.All.SendAsync(RTMethod,
            generatedInvoice.Id, generatedInvoice.Location);
    }
}
