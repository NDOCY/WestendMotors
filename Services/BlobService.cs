using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace WestendMotors.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private const string ContainerName = "westendmotors";

        public BlobService()
        {
            // Get connection string from Web.config
            var connectionString = System.Configuration.ConfigurationManager.AppSettings["AzureBlobConnection"];
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadImageAsync(HttpPostedFileBase file, string folderName = "vehicles")
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // Generate unique filename with folder structure
                var fileName = $"{folderName}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(fileName);

                // Set content type
                var options = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    }
                };

                using (var stream = file.InputStream)
                {
                    await blobClient.UploadAsync(stream, options);
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Blob upload failed: {ex}");
                throw;
            }
        }

        public async Task<string> UploadTradeInImageAsync(HttpPostedFileBase file)
        {
            return await UploadImageAsync(file, "tradeins");
        }

        public async Task<string> UploadVehicleImageAsync(HttpPostedFileBase file)
        {
            return await UploadImageAsync(file, "vehicles");
        }

        public async Task DeleteImageAsync(string blobUrl)
        {
            try
            {
                var uri = new Uri(blobUrl);
                var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

                // C# 7.3 compatible way to get blob name
                var blobName = GetBlobNameFromUrl(blobUrl);

                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Blob deletion failed: {ex}");
                throw;
            }
        }

        // Helper method to extract blob name from URL (C# 7.3 compatible)
        private string GetBlobNameFromUrl(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            // Get the path and remove leading slash
            var path = uri.AbsolutePath;
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            // Remove container name from path
            if (path.StartsWith(ContainerName + "/"))
            {
                path = path.Substring(ContainerName.Length + 1);
            }

            return path;
        }
    }
}