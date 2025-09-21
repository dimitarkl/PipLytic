using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using server.Models;
using server.Services.Mappers;
using server.Utils;

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

    public async Task<StocksSearchResponseDto> QueryStocksData(MarketDataDto request)
    {
        var cacheKey = CacheUtils.GenerateMarketDataCacheKey(request.Symbol, request.Interval);
        var ttl = CacheUtils.MarketDataTtl;

        //8 calls a minute/800 a day
        if (!_cache.TryGetValue(cacheKey, out string jsonString))
        {
            var url = BuildTimeSeriesUrl(request);
            Console.WriteLine(url);
            var response = await SendRequestAsync(url);

            _cache.Set(cacheKey, response, ttl);
            jsonString = response;
        }

        var twelveDataResponse = JsonUtils.ParseResponse<TimeSeriesResponse>(jsonString);

        return MarketDataMapper.MapToClientResponse(twelveDataResponse);
    }

    private string BuildTimeSeriesUrl(MarketDataDto request)
    {
        var (startDate, endDate) = DateTimeUtils.GenerateRandomMonth();
        var queryParams = new Dictionary<string, string?>
        {
            ["symbol"] = request.Symbol,
            ["interval"] = request.Interval,
            ["apikey"] = _configuration.GetValue<string>("AppSettings:TwelveDataApiKey"),
            ["start_date"] = startDate,
            ["end_date"] = endDate
        };

        return QueryHelpers.AddQueryString(_client.BaseAddress + "time_series", queryParams);
    }

    private async Task<string> SendRequestAsync(string url)
    {
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"API request failed ({response.StatusCode}): {content}");

        return content;
    }
}