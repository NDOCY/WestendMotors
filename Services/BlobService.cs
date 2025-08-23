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

        public BlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }
        /*
        public async Task<string> UploadVehicleImageAsync(HttpPostedFileBase file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            await containerClient.CreateIfNotExistsAsync();

            // Generate unique filename with original extension
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.InputStream)
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }*/
        public async Task<string> UploadVehicleImageAsync(HttpPostedFileBase file)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient("westendmotors");
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // Generate unique filename
                var fileName = $"vehicles/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
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

        public async Task<string> UploadImageAsync(HttpPostedFileBase file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("westendmotors");
            await containerClient.CreateIfNotExistsAsync();

            // Keep the same simple filename generation
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.InputStream)
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            var blobName = Path.GetFileName(uri.LocalPath);

            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}