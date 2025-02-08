using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using AssignmentDevOpsProject_fwald.Services;
using Microsoft.Extensions.Configuration;
using System;

public static class QueueTriggerFunctionStartingJob
{
    [FunctionName("ProcessStartJobQueue")]
    public static async Task ProcessStartJobQueue(
        [QueueTrigger("input-queue", Connection = "AzureWebJobsStorage")] string queueItem,
        [Queue("output-queue", Connection = "AzureWebJobsStorage")] ICollector<string> outputQueue,
        ILogger log,
        IHttpClientFactory httpClientFactory,
        ExecutionContext context) 
    {
        log.LogInformation($"Processing queue item: {queueItem}");

        string unsplashApiKey = Environment.GetEnvironmentVariable("UnsplashApiKey");

        try
        {
            var httpClient = httpClientFactory.CreateClient();
            var buienradarAPI = new BuienraderAPI(httpClient);
            var unsplashAPI = new UnsplashAPI(httpClient, unsplashApiKey);

            var weatherData = await buienradarAPI.GetWeatherDataAsync();
            var imageUrls = await unsplashAPI.GetImageUrlsAsync();

            if (imageUrls != null)
            {
                foreach (var imageUrl in imageUrls)
                {
                    string imageProcessingInfo = $"{{\"weatherData\": \"{weatherData}\", \"imageUrl\": \"{imageUrl}\"}}";
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
        }
    }
}
