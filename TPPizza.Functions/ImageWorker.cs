using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace TPPizza.Functions
{
    public class ImageWorker
    {
        private readonly IConfiguration _configuration;

        public ImageWorker(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("ImageResizer")]
        public async Task RunAsync([QueueTrigger("blobs", Connection = "AzureWebJobsStorage")] string myQueueItem)
        {
            var item = JsonSerializer.Deserialize<Message>(myQueueItem);

            var connectionString = _configuration["AzureWebJobsStorage"];

            BlobContainerClient blobContainerClient = new BlobContainerClient(connectionString, "images");

            await blobContainerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = blobContainerClient.GetBlobClient(item.Name);

            var ms = new MemoryStream();
            await blobClient.DownloadToAsync(ms);

            if (item.Extension.ToLower() == ".png" || item.Extension.ToLower() == ".jpg" || item.Extension.ToLower() == ".jpeg")
            {
                using var image = Image.FromStream(ms);
                var thumbs = ResizeImage(image, new Size(100, 100));

                blobContainerClient = new BlobContainerClient(connectionString, "images-thumbs");

                await blobContainerClient.CreateIfNotExistsAsync();

                blobClient = blobContainerClient.GetBlobClient(item.Name);

                var stream = new MemoryStream();
                thumbs.Save(stream, image.RawFormat);

                //Rembobinage du stream
                stream.Position = 0;

                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = item.ContentType});
            }
        }

        private static Image ResizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent, nPercentW, nPercentH = 0;

            nPercentW = (float)size.Width / (float)sourceWidth;
            nPercentH = (float)size.Height / (float)sourceHeight;

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new(destWidth, destHeight);
            Graphics g = Graphics.FromImage(b);

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return b;
        }
    }
}
