using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AssignmentDevOpsProject_fwald.Services
{
    public class BuienraderAPI
    {
        private readonly HttpClient _httpClient;

        public BuienraderAPI(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetWeatherDataAsync()
        {
            try
            {
                string apiUrl = "https://data.buienradar.nl/2.0/feed/json";

                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var measurements = json["actual"]?["stationmeasurements"];
                if (measurements == null || !measurements.HasValues)
                {
                    Console.WriteLine("No station measurements found.");
                    return null;
                }

                var station = measurements[0];
                string stationName = station?["stationname"]?.ToString() ?? "Unknown";
                string temperature = station?["temperature"]?.ToString() ?? "N/A";
                string weatherDescription = station?["weatherdescription"]?.ToString() ?? "N/A";
                string humidity = station?["humidity"]?.ToString() ?? "N/A";
                string windSpeed = station?["windspeedBft"]?.ToString() ?? "N/A";

                return $"{stationName}: {temperature}°C, {weatherDescription}, Humidity: {humidity}%, Wind: {windSpeed} Bft";
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error fetching weather data: {e.Message}");
                return null;
            }
        }
    }
}
