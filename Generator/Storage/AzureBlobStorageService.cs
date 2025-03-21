using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace Generator.Storage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IOptions<AzureBlobStorageOptions> options, ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        var config = options.Value;

        var blobClient = new BlobServiceClient(config.ConnectionString);
        _containerClient = blobClient.GetBlobContainerClient(config.ContainerName);
        _logger.LogInformation("Creating blob container if it does not exist: {}", config.ContainerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task<Uri> UploadAsync(string blobName, Stream content, string contentType = "application/pdf")
    {
        content.Position = 0;
        var blobClient = _containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });

        _logger.LogInformation("Uploaded document to: {}", blobClient.Uri);
        return new Uri(blobClient.Uri.ToString());
    }
}

public class AzureBlobStorageOptions
{
    public string? ConnectionString { get; set; }
    public string? ContainerName { get; set; }
}