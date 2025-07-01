using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
    {
        _blobServiceClient = new BlobServiceClient(configuration["AzureStorage:ConnectionString"]);
        _containerName = configuration["AzureStorage:ContainerName"] ?? "products";
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(fileName);

        await using var stream = file.OpenReadStream();
        
        // Create BlobUploadOptions and set the ContentType via HttpHeaders
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
        };

        // Pass the options object to UploadAsync. Overwriting is the default.
        await blobClient.UploadAsync(stream, uploadOptions);

        _logger.LogInformation("File {FileName} uploaded to Azure Blob Storage.", fileName);
        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync();
        _logger.LogInformation("File {FileName} deleted from Azure Blob Storage.", fileName);
    }
}