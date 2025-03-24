using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace Generator.Storage;

public class AzureBlobStorageService(BlobContainerClient _containerClient, ILogger<AzureBlobStorageService> _logger) : IBlobStorageService
{
    public async Task<Uri> UploadAsync(string blobName, Stream content, string contentType = "application/pdf")
    {
        content.Position = 0;
        var blobClient = _containerClient.GetBlobClient(blobName);
        _logger.LogInformation("Uploading {url}", blobClient.Uri);

        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });

        _logger.LogInformation("Uploaded {url}", blobClient.Uri);
        return new Uri(blobClient.Uri.ToString());
    }
}
