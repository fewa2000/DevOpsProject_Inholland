using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using AssignmentDevOpsProject_fwald.Services;
using Microsoft.AspNetCore.Mvc;

public class ProcessAndUploadImageFunction
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProcessAndUploadImageFunction(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [FunctionName("ProcessAndUploadImageFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("HTTP trigger function to process and upload image started.");

        var httpClient = _httpClientFactory.CreateClient();
        string unsplashApiKey = Environment.GetEnvironmentVariable("UnsplashApiKey") ?? "";

        var buienradarAPI = new BuienraderAPI(httpClient);
        var unsplashAPI = new UnsplashAPI(_httpClientFactory.CreateClient(), unsplashApiKey);
        var blobStorage = new BlobStorage("AzureWebJobsStorage");

        var weatherData = await buienradarAPI.GetWeatherDataAsync();
        var imageData = await unsplashAPI.GetImageDataAsync();

        if (imageData != null && weatherData != null)
        {
            using var imageStream = new MemoryStream(imageData);
            var processedImageStream = ImageHelper.AddTextToImage(
                imageStream,
                (weatherData, (10, 10), 24, "#FFFFFF"));

            if (processedImageStream == null)
            {
                return new BadRequestObjectResult("Failed to process image.");
            }

            string containerName = Environment.GetEnvironmentVariable("BlobContainer") ?? "processed-images";
            await blobStorage.UploadImageAsync(processedImageStream, containerName, "processedImage.jpg");

            return new OkObjectResult("Image processed and uploaded successfully.");
        }

        return new BadRequestObjectResult("Failed to retrieve weather data or image.");
    }
}
