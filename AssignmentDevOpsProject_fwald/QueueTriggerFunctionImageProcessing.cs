using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using AssignmentDevOpsProject_fwald.Services;

public static class QueueTriggerFunctionImageProcessing
{
    [FunctionName("ProcessImageQueue")]
    public static async Task ProcessImageQueue(
        [QueueTrigger("input-queue", Connection = "AzureWebJobsStorage")] string imageInfoJson,
        ILogger log)
    {
        log.LogInformation($"Processing image: {imageInfoJson}");

        var imageInfo = JsonConvert.DeserializeObject<ImageInfo>(imageInfoJson);

        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync(imageInfo.ImageUrl);
        response.EnsureSuccessStatusCode();
        var imageStream = await response.Content.ReadAsStreamAsync();

        var processedImageStream = ImageEditor.ImageHelper.AddTextToImage(imageStream, (imageInfo.WeatherData, (10, 10), 24, "#FFFFFF"));

        var blobStorage = new BlobStorage("AzureWebJobsStorage");

        string blobName = $"processed_image_{DateTime.UtcNow.Ticks}.jpg";
        await blobStorage.UploadImageAsync(processedImageStream, "BlobContainer", blobName);

        log.LogInformation("Image processed and uploaded successfully.");
    }

    public class ImageInfo
    {
        public string WeatherData { get; set; }
        public string ImageUrl { get; set; }
    }
   
}
