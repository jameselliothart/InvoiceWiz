using Contracts;
using Generator.Storage;
using MassTransit;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Generator.Invoices;

public class InvoiceRequestedConsumer(
    IPublishEndpoint _publishEndpoint,
    IBlobStorageService _storage,
    ILogger<InvoiceRequestedConsumer> _logger
    ) : IConsumer<InvoiceRequestedEvent>
{
    public async Task Consume(ConsumeContext<InvoiceRequestedEvent> context)
    {
        var requestedInvoice = context.Message;
        _logger.LogInformation("Generating invoice for {}", requestedInvoice);

        using var pdf = new PdfDocument();
        var page = pdf.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        gfx.DrawString(requestedInvoice.To, new XFont("Arial", 12), XBrushes.Black, new XPoint(10, 10));

        using var stream = new MemoryStream();
        pdf.Save(stream, false);
        stream.Position = 0;

        // Upload to storage
        var location = $"invoices/{requestedInvoice.Id}.pdf";
        _logger.LogInformation("Uploading to {}", location);
        var pdfLink = await _storage.UploadAsync(location, stream);

        // Publish event
        var generatedInvoice = new InvoiceGeneratedEvent(requestedInvoice.Id, pdfLink);
        _logger.LogInformation("Publishing {}", generatedInvoice);
        await _publishEndpoint.Publish(generatedInvoice);
    }
}
