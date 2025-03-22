using Contracts;

namespace Generator.FileGeneration;

public interface IFileGenerator
{
    MemoryStream Generate(InvoiceRequestedEvent requestedInvoice);
}