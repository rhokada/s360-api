using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebApi.Helpers
{
    public class StorageConfig
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string ImageProducts { get; set; }
        public string ImageContactForm { get; set; }
        public string ImagePurchaseReceipt { get; set; }
        public string s360audio { get; set; }
        public string s360images { get; set; }
    }

    public class StorageHelpers
    {
        public static async Task<string> upload(string base64, string filename, StorageConfig storageConfig, string containerName)
        {
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={storageConfig.AccountName};AccountKey={storageConfig.AccountKey};EndpointSuffix=core.windows.net";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filename);

            var imageBytes = Convert.FromBase64String(base64);
            using var stream = new MemoryStream(imageBytes);
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }
    }
}
