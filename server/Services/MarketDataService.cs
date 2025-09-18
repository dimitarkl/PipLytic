using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using server.Models;

namespace server.Services;

public class MarketDataService : IMarketDataService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    public MarketDataService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IMemoryCache cache)
    {
        _client = httpClientFactory.CreateClient("TwelveData");
        _configuration = configuration;
        _cache = cache;
    }

    public async Task<TimeSeriesResponseDto> QueryStocksData(MarketDataDto request)
    {
        var cacheKey = $"{request.Symbol}:{request.Interval}";

        var ttl = TimeSpan.FromMinutes(3);
        
        //8 calls a minute/800 a day
        
        if (!_cache.TryGetValue(cacheKey, out string json))
        {

            var url = BuildTimeSeriesUrl(request);
            Console.WriteLine(url);
            var response = await SendRequestAsync(url);

            _cache.Set(cacheKey, response, ttl);
            json = response;
        }

        return ParseResponse<TimeSeriesResponseDto>(json);
    }

    private static (string StartDate, string EndDate) GenerateRandomMonth()
    {
        var start = new DateTime(2020, 2, 11, 0, 0, 0, DateTimeKind.Utc);
        var now = DateTime.UtcNow;
        var endMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalMonths = (endMonthStart.Year - start.Year) * 12 + (endMonthStart.Month - start.Month);

        var offset = Random.Shared.Next(0, totalMonths + 1);

        var randomMonth = start.AddMonths(offset);

        var monthStart = new DateTime(randomMonth.Year, randomMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        return (
            monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            monthEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        );
    }

    private string BuildTimeSeriesUrl(MarketDataDto request)
    {
        var (startDate, endDate) = GenerateRandomMonth();
        var queryParams = new Dictionary<string, string?>
        {
            ["symbol"] = request.Symbol,
            ["interval"] = request.Interval,
            ["apikey"] = _configuration.GetValue<string>("AppSettings:TwelveDataApiKey"),
            ["start_date"] = startDate,
            ["end_date"] = endDate
        };

        return QueryHelpers.AddQueryString(_client.BaseAddress +"time_series", queryParams);
    }

    private async Task<string> SendRequestAsync(string url)
    {
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"API request failed ({response.StatusCode}): {content}");

        return content;
    }

    private T ParseResponse<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentNullException(nameof(json), "Twelve Data returned an empty response.");
        
        if (json.Contains("\"status\":\"error\"", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Twelve Data API returned an error: " + json);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        try
        {
            var result = JsonSerializer.Deserialize<T>(json, options);
            if (result == null)
                throw new Exception("Failed to deserialize Twelve Data response.");

            // Optional: further check for expected data
            // If T is TwelveDataResponseDto, make sure Values exist
            if (result is TimeSeriesResponseDto dto && (dto.Values == null || dto.Values.Count == 0))
                throw new Exception("Twelve Data returned no values.");

            return result;
        }
        catch (JsonException ex)
        {
            throw new Exception("Invalid JSON returned from Twelve Data.", ex);
        }
    }

}