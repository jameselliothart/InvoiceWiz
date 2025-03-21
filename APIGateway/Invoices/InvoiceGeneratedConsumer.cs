using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
namespace APIGateway.Invoices;

public class InvoiceGeneratedConsumer(IHubContext<InvoiceHub> _hubContext) : IConsumer<InvoiceGeneratedEvent>
{
    public async Task Consume(ConsumeContext<InvoiceGeneratedEvent> context)
    {
        await _hubContext.Clients.All.SendAsync("InvoiceGenerated",
            context.Message.Id, context.Message.Location);
    }
}
