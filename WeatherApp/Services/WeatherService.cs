using System.Text.Json;
using System.Net.Http;
using WeatherApp.Pages; // This using statement is critical

namespace WeatherApp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = "d42ed1911a6ca7ba5743c568e247fd38";
        }

        public async Task<WeatherData?> GetCurrentWeatherAsync(string city)
        {
            string url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<WeatherData>(content);
            }
            return null;
        }

        public async Task<WeatherApiResponse?> GetForecastAsync(string city)
        {
            string url = $"http://api.openweathermap.org/data/2.5/forecast?q={city}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<WeatherApiResponse>(content);
            }
            return null;
        }
    }
}