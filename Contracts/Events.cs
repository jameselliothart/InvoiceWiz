namespace Contracts;

public record InvoiceRequestedEvent(Guid Id, string To, decimal Amount)
{
    public DateTimeOffset Date { get; set; }
}

public record InvoiceGeneratedEvent(Guid Id, Uri Location);
