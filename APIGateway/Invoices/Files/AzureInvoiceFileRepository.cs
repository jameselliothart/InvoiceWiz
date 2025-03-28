namespace APIGateway.Invoices.Files;

public class AzureInvoiceFileRepository(
    IHttpClientFactory httpClientFactory,
    ILogger<AzureInvoiceFileRepository> _logger
    ) : IInvoiceFileRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient();
    private const string msVersionKey = "x-ms-version";
    private const string msVersionValue = "2020-04-08";
    private const string accountName = "devstoreaccount1";
    private const string accountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
    private const string azureUrlTemplate = "http://azurite:10000/devstoreaccount1/invoices/{0}.pdf";

    public async Task<IAzureResult> Get(Guid id)
    {
        var url = new Uri(string.Format(azureUrlTemplate, id));
        using (_logger.BeginScope(new Dictionary<string, object>{["url"] = url}))
        {
            _logger.LogInformation("Downloading");
            // Add SharedKey auth (for Azurite demo only)
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(msVersionKey, msVersionValue); // Azurite-compatible version
            request.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("R")); // RFC 1123
            request.Headers.Add("Authorization", GenerateSharedKeyAuth(url, "GET"));

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download: {statusCode}", response.StatusCode);
                return new DownloadFailure((int)response.StatusCode);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var fileName = Path.GetFileName(url.ToString());
            _logger.LogInformation("Downloaded");
            return new DownloadSuccess(stream, fileName);
        }
    }

    // Simple SharedKey auth (for demo, not production-ready)
    static string GenerateSharedKeyAuth(Uri uri, string method)
    {
        var stringToSign = $"{method}\n\n\n\n\n\n\n\n\n\n\n\nx-ms-date:{DateTime.UtcNow:R}\n{msVersionKey}:{msVersionValue}\n/{accountName}{uri.AbsolutePath}";
        using var hmac = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(accountKey));
        var signature = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToSign)));
        return $"SharedKey {accountName}:{signature}";
    }
}
