using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using WeatherApp.Services;

namespace WeatherApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly WeatherService _weatherService;

        public IndexModel(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [BindProperty]
        public string City { get; set; }

        public string ErrorMessage { get; set; }

        public WeatherData? WeatherData { get; set; }

        public WeatherApiResponse? ForecastData { get; set; }

        public async Task OnPostAsync()
        {
            string graphPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "forecast_graph.png");
            if (System.IO.File.Exists(graphPath))
            {
                System.IO.File.Delete(graphPath);
            }

            if (string.IsNullOrEmpty(City))
            {
                ErrorMessage = "Please enter a city name.";
                return;
            }

            try
            {
                // Get data from the API calls
                WeatherData = await _weatherService.GetCurrentWeatherAsync(City);
                ForecastData = await _weatherService.GetForecastAsync(City);

                // Check for API errors
                if (WeatherData == null || ForecastData == null)
                {
                    ErrorMessage = "Could not find weather data for that city. Please check the city name and spelling.";
                    return;
                }

                // Serialize the forecast data to a JSON string
                string forecastJson = JsonSerializer.Serialize(ForecastData);

                // Path to your Python script
                string scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "generate_graph.py");

                // Set up the process to run the Python script
                var startInfo = new ProcessStartInfo
                {
                    FileName = "py",
                    Arguments = $"\"{scriptPath}\" \"{forecastJson.Replace("\"", "\\\"")}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true, // capture errors
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (!string.IsNullOrEmpty(error))
                    {
                        ErrorMessage = $"Python script error: {error}";
                    }
                    else
                    {
                        Console.WriteLine(output);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
        }
    }

    // These data model classes MUST be defined outside of the IndexModel class.
    // They are now public, which allows them to be used by other classes (like WeatherService).

    public class WeatherData
    {
        public MainData main { get; set; }
        public List<WeatherCondition> weather { get; set; }
        public WindData wind { get; set; }
    }

    public class MainData
    {
        public double temp { get; set; }
        public int humidity { get; set; }
        public double pressure { get; set; }
        public double feels_like { get; set; }
    }

    public class WeatherCondition
    {
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class WindData
    {
        public double speed { get; set; }
    }

    public class WeatherApiResponse
    {
        public List<ForecastEntry> list { get; set; }
    }

    public class ForecastEntry
    {
        public MainData main { get; set; }
        public List<WeatherCondition> weather { get; set; }
        public WindData wind { get; set; }
        public string dt_txt { get; set; }
    }
}