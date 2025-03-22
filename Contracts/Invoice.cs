namespace Contracts;

public record Invoice(Guid Id, string To, decimal Amount, Uri Location, DateTimeOffset CreatedDate, string Details, DateOnly InvoiceDate) {}
