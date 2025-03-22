namespace APIGateway.Invoices.Files;

public interface IAzureResult {}
public record DownloadFailure(int StatusCode) : IAzureResult;
public record DownloadSuccess(Stream Stream, string FileName) : IAzureResult;