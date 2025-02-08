using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AssignmentDevOpsProject_fwald.Services
{
    internal class BuienraderAPI
    {
        private HttpClient _httpClient;

        public BuienraderAPI(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
            public async Task<byte[]> GetWeatherDataAsync()
            {
                try
                {
                    string apiUrl = $"https://data.buienradar.nl/2.0/feed/json";

                    var response = await _httpClient.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    string content = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(content);
                    string imageUrl = json["actual"]["stationmeasurements"]?.ToString();

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        var imageResponse = await _httpClient.GetAsync(imageUrl);
                        imageResponse.EnsureSuccessStatusCode();

                        byte[] imageData = await imageResponse.Content.ReadAsByteArrayAsync();
                        return imageData; 
                    }
                    else
                    {
                        Console.WriteLine("No images found");
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error fetching image: {e.Message}");
                    return null;
                }
            }

    }
}

