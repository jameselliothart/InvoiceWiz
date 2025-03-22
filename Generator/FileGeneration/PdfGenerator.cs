using Contracts;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Generator.FileGeneration;

public class PdfGenerator : IFileGenerator
{
    public MemoryStream Generate(InvoiceRequestedEvent requestedInvoice)
    {
        using var document = new PdfDocument();
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        // Define constants
        const double margin = 40;
        double currentY = margin;

        // Define fonts
        var headerFont = new XFont("DejaVu Sans", 14, XFontStyle.Bold);
        var titleFont = new XFont("DejaVu Sans", 24, XFontStyle.Bold);
        var regularFont = new XFont("DejaVu Sans", 10, XFontStyle.Regular);

        // Placeholder company information
        string companyName = "InvoiceWiz";
        string companyAddress = "123 Business St, City, Country";
        string companyContact = "contact@yourcompany.com";

        // Draw company header
        gfx.DrawString(companyName, headerFont, XBrushes.Black,
            new XRect(margin, currentY, page.Width - 2 * margin, 20), XStringFormats.TopLeft);
        currentY += 20;

        gfx.DrawString(companyAddress, regularFont, XBrushes.Black,
            new XRect(margin, currentY, page.Width - 2 * margin, 15), XStringFormats.TopLeft);
        currentY += 15;

        gfx.DrawString(companyContact, regularFont, XBrushes.Black,
            new XRect(margin, currentY, page.Width - 2 * margin, 15), XStringFormats.TopLeft);
        currentY += 30;

        // Draw "INVOICE" title
        gfx.DrawString("INVOICE", titleFont, XBrushes.Black,
            new XRect(margin, currentY, page.Width - 2 * margin, 40), XStringFormats.TopLeft);
        currentY += 40;

        // Draw recipient and date
        gfx.DrawString($"To: {requestedInvoice.To}", regularFont, XBrushes.Black,
            new XRect(margin, currentY, 200, 20), XStringFormats.TopLeft);

        string formattedDate = requestedInvoice.InvoiceDate.ToString("MMMM dd, yyyy");
        gfx.DrawString($"Date: {formattedDate}", regularFont, XBrushes.Black,
            new XRect(page.Width - margin - 200, currentY, 200, 20), XStringFormats.TopRight);
        currentY += 20;

        // Draw separator line
        gfx.DrawLine(XPens.Black, margin, currentY, page.Width - margin, currentY);
        currentY += 10;

        // Draw table headers
        gfx.DrawString("Description", regularFont, XBrushes.Black,
            new XRect(margin, currentY, 300, 15), XStringFormats.TopLeft);
        gfx.DrawString("Amount", regularFont, XBrushes.Black,
            new XRect(page.Width - margin - 100, currentY, 100, 15), XStringFormats.TopLeft);
        currentY += 15;

        // Draw invoice item
        gfx.DrawString(requestedInvoice.Details, regularFont, XBrushes.Black,
            new XRect(margin, currentY, 300, 15), XStringFormats.TopLeft);
        string formattedAmount = requestedInvoice.Amount.ToString("0.00");
        gfx.DrawString(formattedAmount, regularFont, XBrushes.Black,
            new XRect(page.Width - margin - 100, currentY, 100, 15), XStringFormats.TopLeft);
        currentY += 20;

        // Draw separator line
        gfx.DrawLine(XPens.Black, margin, currentY, page.Width - margin, currentY);
        currentY += 10;

        // Draw total
        gfx.DrawString("Total:", regularFont, XBrushes.Black,
            new XRect(page.Width - margin - 150, currentY, 50, 15), XStringFormats.TopLeft);
        gfx.DrawString(formattedAmount, regularFont, XBrushes.Black,
            new XRect(page.Width - margin - 100, currentY, 100, 15), XStringFormats.TopLeft);
        currentY += 30;

        // Draw thank-you note
        gfx.DrawString("Thank you for your business.", regularFont, XBrushes.Black,
            new XRect(margin, currentY, page.Width - 2 * margin, 15), XStringFormats.TopLeft);

        var stream = new MemoryStream();
        document.Save(stream, false);
        stream.Position = 0;
        return stream;
    }
}
