using System.ComponentModel;
using System.Net.Http.Json;
using Microsoft.SemanticKernel;

namespace Console_SK_Planner.Plugins
{
    public class WeatherResponse
    {
        public MainInfo? Weather { get; set; }
    }

    public class MainInfo
    {
        public double temp { get; set; }
        public double feels_like { get; set; }
    }

    public class WeatherPlugin
    {
        const string WeatherPluginEndpoint = "https://api.openweathermap.org/data/2.5/weather?zip=";
        const string WeatherAPIKey = "3f8643931dc30aacc44cee06d2c9b5bd";
        const string units = "imperial";
        // https://api.openweathermap.org/data/2.5/weather?zip=28012,us&appid=3f8643931dc30aacc44cee06d2c9b5bd&units=imperial
        // by zip code is the most accurate.
        // api.openweathermap.org/data/2.5/weather?q=Belmont,US-ND&APPID=3f8643931dc30aacc44cee06d2c9b5bd&units=imperial
        // API Key 3f8643931dc30aacc44cee06d2c9b5bd

        private readonly HttpClient _client = new();

        [KernelFunction]
        [Description("Retreives the current weather for any zip code in the US")]
        public async Task<string> GetWeatherAsync([Description("zip code for the weather")] string zipcode)
        {
            var openWeatherEndpoint = $@"{WeatherPluginEndpoint}{zipcode},us&appid={WeatherAPIKey}&units={units}";
            HttpRequestMessage request = new(HttpMethod.Get, openWeatherEndpoint);

            var response = await _client.SendAsync(request).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return result;
        }
    }
}
