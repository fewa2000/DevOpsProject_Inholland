using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AssignmentDevOpsProject_fwald.Services
{
    public class UnsplashAPI
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public UnsplashAPI(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<byte[]?> GetImageDataAsync()
        {
            try
            {
                string apiUrl = "https://api.unsplash.com/photos/random";
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Client-ID", _apiKey);

                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                string? imageUrl = json["urls"]?["regular"]?.ToString();

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var imageResponse = await _httpClient.GetAsync(imageUrl);
                    imageResponse.EnsureSuccessStatusCode();

                    byte[] imageData = await imageResponse.Content.ReadAsByteArrayAsync();
                    return imageData;
                }
                else
                {
                    Console.WriteLine("No image URL found in Unsplash response.");
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error fetching image: {e.Message}");
                return null;
            }
        }

        public async Task<List<string>?> GetImageUrlsAsync(int count = 5)
        {
            try
            {
                string apiUrl = $"https://api.unsplash.com/photos/random?count={count}";
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Client-ID", _apiKey);

                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var jsonArray = JArray.Parse(content);

                var urls = new List<string>();
                foreach (var photo in jsonArray)
                {
                    string? url = photo["urls"]?["regular"]?.ToString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        urls.Add(url);
                    }
                }

                return urls.Count > 0 ? urls : null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error fetching image URLs: {e.Message}");
                return null;
            }
        }
    }
}
