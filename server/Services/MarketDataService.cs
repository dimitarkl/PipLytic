using System.Globalization;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using server.Models;

namespace server.Services;

public class MarketDataService : IMarketDataService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _configuration;

    public MarketDataService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _client = httpClientFactory.CreateClient("AlphaVantage");
        _configuration = configuration;
    }

    public async Task<IntradayResponseDto> QueryStocksData(MarketDataDto request)
    {
        var url = BuildUrl(request);
        var json = await SendRequestAsync(url);
        return ParseResponse<IntradayResponseDto>(json);
    }

    private static string GenerateRandomMonth()
    {
        var start = new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var now = DateTime.UtcNow;
        var endMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalMonths = (endMonthStart.Year - start.Year) * 12 + (endMonthStart.Month - start.Month);

        var offset = Random.Shared.Next(0, totalMonths + 1);

        var randomMonth = start.AddMonths(offset);
        return randomMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture);
    }

    private string BuildUrl(MarketDataDto request)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["function"] = "TIME_SERIES_INTRADAY",
            ["symbol"] = request.symbol,
            ["interval"] = request.interval,
            ["apikey"] = _configuration.GetValue<string>("AppSettings:AlphaVantageApiKey"),
            ["month"] = GenerateRandomMonth()
        };

        return QueryHelpers.AddQueryString(_client.BaseAddress + "query", queryParams);
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
        if (json.Contains("\"Error Message\"", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Alpha Vantage API returned an error: " + json);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<T>(json, options)
               ?? throw new Exception("Failed to deserialize Alpha Vantage response.");
    }
}