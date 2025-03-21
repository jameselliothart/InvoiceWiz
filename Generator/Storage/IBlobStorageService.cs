namespace Generator.Storage;

public interface IBlobStorageService
{
    Task<Uri> UploadAsync(string blobName, Stream content, string contentType = "application/pdf");
}