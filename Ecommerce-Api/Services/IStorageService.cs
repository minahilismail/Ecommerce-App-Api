namespace Ecommerce_Api.Services
{
    public interface IStorageService
    {
        Task<string> UploadAsync(byte[] fileData, string fileName, string containerName = "");
    }
}
