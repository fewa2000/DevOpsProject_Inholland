using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AssignmentDevOpsProject_fwald.Services
{
    public class BlobStorage
    {
        private readonly string _storageConnectionString;
        public BlobStorage(string storageConnectionString)
        {
            _storageConnectionString = storageConnectionString;
        }
        public async Task UploadImageAsync(Stream imageStream, string blobContainerName, string fileName)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(_storageConnectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);

                await blobContainerClient.CreateIfNotExistsAsync();

                var blobClient = blobContainerClient.GetBlobClient(fileName);

                imageStream.Position = 0;

                await blobClient.UploadAsync(imageStream, overwrite: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading to Blob Storage: {ex.Message}");
                throw;
            }
        }
    }
}
