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

        using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["invoiceId"] = requestedInvoice.Id,
                ["requestedInvoice"] = requestedInvoice,
            }
        ))
        {
            _logger.LogInformation("Generating");
            using var stream = _generator.Generate(requestedInvoice);
            _logger.LogInformation("Generated");

            // Upload to storage
            var blobName = $"{requestedInvoice.Id}.pdf";
            _logger.LogInformation("Uploading {blobName}", blobName);
            var pdfLink = await _storage.UploadAsync(blobName, stream);
            _logger.LogInformation("Uploaded {blobName}", blobName);

            // Publish event
            var generatedInvoice = new InvoiceGeneratedEvent(requestedInvoice.Id, pdfLink);
            using (_logger.BeginScope(new Dictionary<string, object>{["generatedInvoice"] = generatedInvoice}))
            {
                _logger.LogInformation("Publishing");
                await _publishEndpoint.Publish(generatedInvoice);
                _logger.LogInformation("Published");
            }
        }
    }
}
