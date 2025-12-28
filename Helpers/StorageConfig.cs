using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static async Task<string> upload(string base64, string filename, StorageConfig storageConfig, string folder)
        {
            var storageCredentials = new StorageCredentials(storageConfig.AccountName, storageConfig.AccountKey);
            var storageAccount = new CloudStorageAccount(storageCredentials, true);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(folder);
            var blockBlob = container.GetBlockBlobReference(filename);

            var image = Convert.FromBase64String(base64);
            await blockBlob.UploadFromByteArrayAsync(image, 0, image.Length);

            return blockBlob.SnapshotQualifiedStorageUri.PrimaryUri.ToString();
        }
    }

}
