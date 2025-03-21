namespace Persister.Invoices;

public record Invoice(Guid Id, string To, decimal Amount, Uri Location, DateTimeOffset CreatedDate) {}
