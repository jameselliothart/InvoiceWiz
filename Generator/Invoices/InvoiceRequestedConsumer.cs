using Contracts;
using Generator.FileGeneration;
using Generator.Storage;
using MassTransit;

namespace Generator.Invoices;

public class InvoiceRequestedConsumer(
    IPublishEndpoint _publishEndpoint,
    IBlobStorageService _storage,
    IFileGenerator _generator,
    ILogger<InvoiceRequestedConsumer> _logger
    ) : IConsumer<InvoiceRequestedEvent>
{
    public async Task Consume(ConsumeContext<InvoiceRequestedEvent> context)
    {
        var requestedInvoice = context.Message;
        _logger.LogInformation("Generating invoice for {}", requestedInvoice);

        var stream = _generator.Generate(requestedInvoice);

        // Upload to storage
        var location = $"{requestedInvoice.Id}.pdf";
        _logger.LogInformation("Uploading to {}", location);
        var pdfLink = await _storage.UploadAsync(location, stream);

        // Publish event
        var generatedInvoice = new InvoiceGeneratedEvent(requestedInvoice.Id, pdfLink);
        _logger.LogInformation("Publishing {}", generatedInvoice);
        await _publishEndpoint.Publish(generatedInvoice);
    }
}
