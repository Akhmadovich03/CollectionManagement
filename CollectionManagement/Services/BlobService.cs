using Azure.Storage.Blobs;

namespace CollectionManagement.Services;

public class BlobService
{
    private readonly BlobContainerClient _containerClient;

    public BlobService(IConfiguration configuration)
    {
        var connectionString = configuration["Azure:BlobStorage:ConnectionString"];
        var containerName = configuration["Azure:BlobStorage:ContainerName"];
        var blobServiceClient = new BlobServiceClient(connectionString);
       
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(imageStream, true);
        
        return blobClient.Uri.ToString();
    }
}