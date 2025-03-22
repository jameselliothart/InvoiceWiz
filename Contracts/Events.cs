namespace Contracts;

public record InvoiceRequestedEvent(Guid Id, string To, decimal Amount, string Details, DateOnly InvoiceDate)
{
    public DateTimeOffset Date { get; set; }
}

public record InvoiceGeneratedEvent(Guid Id, Uri Location);
