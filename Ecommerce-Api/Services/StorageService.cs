using Azure.Storage.Blobs;
using System.Reflection.Metadata;

namespace Ecommerce_Api.Services
{
    public class StorageService : IStorageService
    {
        private BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly string _containerName;
        public StorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            string connectionString = _configuration["AzureStorage:ConnectionString"];
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = _configuration["AzureStorage:ContainerName"];
            this._configuration = configuration;
        }

        public async Task<string> UploadAsync(byte[] fileData, string fileName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(string.IsNullOrEmpty(containerName) ? _containerName : containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = new MemoryStream(fileData))
            {
                await blobClient.UploadAsync(stream, true);
            }
            return blobClient.Uri.ToString();

        }
    
    }
}
