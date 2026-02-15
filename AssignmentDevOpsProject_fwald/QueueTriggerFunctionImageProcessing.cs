using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using AssignmentDevOpsProject_fwald.Services;

public class QueueTriggerFunctionImageProcessing
{
    private readonly IHttpClientFactory _httpClientFactory;

    public QueueTriggerFunctionImageProcessing(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [FunctionName("ProcessImageQueue")]
    public async Task ProcessImageQueue(
        [QueueTrigger("image-processing-queue", Connection = "AzureWebJobsStorage")] string imageInfoJson,
        ILogger log)
    {
        log.LogInformation($"Processing image: {imageInfoJson}");

        var imageInfo = JsonConvert.DeserializeObject<ImageInfo>(imageInfoJson);
        if (imageInfo == null || string.IsNullOrEmpty(imageInfo.ImageUrl))
        {
            log.LogError("Invalid image info received.");
            return;
        }

        var httpClient = _httpClientFactory.CreateClient();

        var response = await httpClient.GetAsync(imageInfo.ImageUrl);
        response.EnsureSuccessStatusCode();
        var imageStream = await response.Content.ReadAsStreamAsync();

        var processedImageStream = ImageHelper.AddTextToImage(
            imageStream,
            (imageInfo.WeatherData ?? "No weather data", (10, 10), 24, "#FFFFFF"));

        if (processedImageStream == null)
        {
            log.LogError("Failed to process image.");
            return;
        }

        var blobStorage = new BlobStorage("AzureWebJobsStorage");

        string containerName = Environment.GetEnvironmentVariable("BlobContainer") ?? "processed-images";
        string blobName = $"processed_image_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.jpg";
        await blobStorage.UploadImageAsync(processedImageStream, containerName, blobName);

        log.LogInformation($"Image processed and uploaded as {blobName}.");
    }

    public class ImageInfo
    {
        public string? WeatherData { get; set; }
        public string? ImageUrl { get; set; }
    }
}
