using Contracts;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Generator.FileGeneration;

public class PdfGenerator : IFileGenerator
{
    public MemoryStream Generate(InvoiceRequestedEvent requestedInvoice)
    {
        using var pdf = new PdfDocument();
        var page = pdf.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        gfx.DrawString(requestedInvoice.To, new XFont("DejaVu Sans", 12), XBrushes.Black, new XPoint(10, 10));

        using var stream = new MemoryStream();
        pdf.Save(stream, false);
        stream.Position = 0;
        return stream;
    }
}
