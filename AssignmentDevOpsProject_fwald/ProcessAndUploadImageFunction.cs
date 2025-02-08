using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;
using AssignmentDevOpsProject_fwald.Services;
using Microsoft.AspNetCore.Mvc;
using ImageEditor;

public static class ProcessAndUploadImageFunction
{
    [FunctionName("ProcessAndUploadImageFunction")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("HTTP trigger function to process and upload image started.");

        var buienradarAPI = new BuienraderAPI(new HttpClient());
        var unsplashAPI = new UnsplashAPI(new HttpClient(), "UnsplashApiKey");
        var blobStorage = new BlobStorage("AzureWebJobsStorage");

        var weatherData = await buienradarAPI.GetWeatherDataAsync();

        var imageData = await unsplashAPI.GetImageDataAsync();

        if (imageData != null && weatherData != null)
        {
            using var imageStream = new MemoryStream(imageData);
            var processedImageStream = ImageHelper.AddTextToImage(imageStream, (weatherData, (10, 10), 24, "#FFFFFF"));

            await blobStorage.UploadImageAsync(processedImageStream, "BlobContainer", "processedImage.jpg");

            return new OkObjectResult("Image processed and uploaded successfully.");
        }
        return new BadRequestObjectResult("Failed to process and upload image.");
    }
}
