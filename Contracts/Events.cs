namespace Contracts;

public record InvoiceRequestedEvent(Guid Id, string To, decimal Amount, DateTimeOffset Date);
public record InvoiceGeneratedEvent(Guid Id, Uri Location);
