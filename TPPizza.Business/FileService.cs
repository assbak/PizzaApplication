using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TPPizza.Business
{
    public class FileService
    {
        private readonly IConfiguration _configuration;

        public FileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task UploadAsync(string blobName, byte[] data, string contentType)
        {
            var connectionString = _configuration.GetConnectionString("AccountStorage");

            BlobContainerClient blobContainerClient = new BlobContainerClient(connectionString, "images");

            await blobContainerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(new BinaryData(data));
            await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType });

            QueueClient queueClient = new QueueClient(connectionString, "blobs", new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });

            await queueClient.CreateIfNotExistsAsync();

            var json = JsonSerializer.Serialize(new
            {
                Name = blobName,
                Extension = Path.GetExtension(blobName),
                ContentType = contentType,
            });

            await queueClient.SendMessageAsync(json);

            // OBJECTIF à TERME

            // Azure Function > Qui se declanche lorsqu'un message est ajouté à la QUEUE
            // Telecharger le blob qui porte le nom mit dans le message de la QUEUE
            // Resize ce blob pour avoir une vignette et non plus l'image d'origine.
        }

        public async Task DeleteAsync(string blobName)
        {
            var connectionString = _configuration.GetConnectionString("AccountStorage");

            BlobContainerClient blobContainerClient = new BlobContainerClient(connectionString, "images");

            var blob = GetBlobByName(blobName, blobContainerClient);

            if (blob != null)
            {
                BlobClient blobClient = blobContainerClient.GetBlobClient(blob.Name);
                await blobClient.DeleteAsync();
            }
        }

        public async Task<byte[]> GetAsync(string blobName)
        {
            var connectionString = _configuration.GetConnectionString("AccountStorage");

            BlobContainerClient blobContainerClient = new BlobContainerClient(connectionString, "images-thumbs");

            // Un blob ayant le nom du fichier existe t'il dans le container ?

            var blob = GetBlobByName(blobName, blobContainerClient);

            if (blob != null)
            {
                BlobClient blobClient = blobContainerClient.GetBlobClient(blob.Name);

                // Le fichier existe on le redescend du container pour transformer du binaire en stream (DownloadTo) puis en byte[] (ToArray)

                using var ms = new MemoryStream();
                await blobClient.DownloadToAsync(ms);

                return ms.ToArray();
            }

            return Array.Empty<byte>();
        }

        private BlobItem? GetBlobByName(string blobName, BlobContainerClient blobContainerClient)
        {
            var blobItems = blobContainerClient.GetBlobs();

            return blobItems.FirstOrDefault(b => Path.GetFileNameWithoutExtension(b.Name) == blobName);
        }
    }
}
