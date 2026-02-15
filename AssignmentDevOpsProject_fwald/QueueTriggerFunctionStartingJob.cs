using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using AssignmentDevOpsProject_fwald.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class QueueTriggerFunctionStartingJob
{
    private readonly IHttpClientFactory _httpClientFactory;

    public QueueTriggerFunctionStartingJob(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [FunctionName("ProcessStartJobQueue")]
    public async Task ProcessStartJobQueue(
        [QueueTrigger("start-job-queue", Connection = "AzureWebJobsStorage")] string queueItem,
        [Queue("image-processing-queue", Connection = "AzureWebJobsStorage")] ICollector<string> outputQueue,
        ILogger log)
    {
        log.LogInformation($"Processing queue item: {queueItem}");

        string unsplashApiKey = Environment.GetEnvironmentVariable("UnsplashApiKey") ?? "";

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var buienradarAPI = new BuienraderAPI(httpClient);
            var unsplashAPI = new UnsplashAPI(_httpClientFactory.CreateClient(), unsplashApiKey);

            var weatherData = await buienradarAPI.GetWeatherDataAsync();
            var imageUrls = await unsplashAPI.GetImageUrlsAsync();

            if (imageUrls != null)
            {
                foreach (var imageUrl in imageUrls)
                {
                    var imageInfo = new { WeatherData = weatherData, ImageUrl = imageUrl };
                    string imageProcessingInfo = JsonConvert.SerializeObject(imageInfo);
                    outputQueue.Add(imageProcessingInfo);
                }
            }
            else
            {
                log.LogWarning("No image URLs found.");
            }
        }
        catch (Exception ex)
        {
            log.LogError($"An error occurred: {ex.Message}");
            throw;
        }
    }
}
