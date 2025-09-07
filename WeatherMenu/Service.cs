using Newtonsoft.Json;
using TelegramBot;

namespace WeatherMenu
{
    public class Service
    {
        private readonly string weather_apiKey = Program.Weather_api_key;
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";
        private static readonly HttpClient _httpClient = new HttpClient();
        public Service(string apiKey)
        {
            weather_apiKey = apiKey;
        }

        public async Task<WeatherData> GetWeatherAsync(string city)
        {
            try
            {
                string url = $"{BaseUrl}?q={Uri.EscapeDataString(city)}&appid={weather_apiKey}&units=metric&lang=ru";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WeatherData>(json);
            }
            catch
            {
                return null;
            }
        }

        public class WeatherData
        {
            [JsonProperty("name")]
            public string City { get; set; }

            [JsonProperty("main")]
            public MainData Main { get; set; }

            [JsonProperty("weather")]
            public WeatherDescription[] Weather { get; set; }

            [JsonProperty("wind")]
            public WindData Wind { get; set; }
        }
        public class MainData
        {
            [JsonProperty("temp")]
            public double Temperature { get; set; }

            [JsonProperty("feels_like")]
            public double FeelsLike { get; set; }

            [JsonProperty("pressure")]
            public int Pressure { get; set; }

            [JsonProperty("humidity")]
            public int Humidity { get; set; }
        }
        public class WeatherDescription
        {
            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("icon")]
            public string Icon { get; set; }
        }
        public class WindData
        {
            [JsonProperty("speed")]
            public double Speed { get; set; }

            [JsonProperty("deg")]
            public int Direction { get; set; }
        }
    }
}