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

        _logger.LogInformation("Generating {requestedInvoice} {invoiceId}", requestedInvoice, requestedInvoice.Id);
        using var stream = _generator.Generate(requestedInvoice);
        _logger.LogInformation("Generated {requestedInvoice} {invoiceId}", requestedInvoice, requestedInvoice.Id);

        // Upload to storage
        var blobName = $"{requestedInvoice.Id}.pdf";
        _logger.LogInformation("Uploading {blobName} {requestedInvoice} {invoiceId}", blobName, requestedInvoice, requestedInvoice.Id);
        var pdfLink = await _storage.UploadAsync(blobName, stream);
        _logger.LogInformation("Uploaded {blobName} {requestedInvoice} {invoiceId}", blobName, requestedInvoice, requestedInvoice.Id);

        // Publish event
        var generatedInvoice = new InvoiceGeneratedEvent(requestedInvoice.Id, pdfLink);
        _logger.LogInformation("Publishing {generatedInvoice} {invoiceId}", generatedInvoice, generatedInvoice.Id);
        await _publishEndpoint.Publish(generatedInvoice);
        _logger.LogInformation("Published {generatedInvoice} {invoiceId}", generatedInvoice, generatedInvoice.Id);
    }
}
