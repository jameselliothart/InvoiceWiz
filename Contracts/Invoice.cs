namespace Contracts;

public record InvoiceOverviewDto(Guid Id, string To, decimal Amount, Uri Location, DateTimeOffset CreatedDate, DateOnly InvoiceDate);

public record Invoice(Guid Id, string To, decimal Amount, Uri Location, DateTimeOffset CreatedDate, string Details, DateOnly InvoiceDate)
    : InvoiceOverviewDto(Id, To, Amount, Location, CreatedDate, InvoiceDate);
