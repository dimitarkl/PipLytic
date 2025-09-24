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
        var requestedIntervalKey = CacheUtils.GenerateMarketDataCacheKey(request.Symbol, request.Interval);
        var ttl = CacheUtils.MarketDataTtl;

        if (_cache.TryGetValue(requestedIntervalKey, out TimeSeriesResponse finalData))
            return MarketDataMapper.MapToClientResponse(finalData);

        var fiveMinCacheKey = CacheUtils.GenerateMarketDataCacheKey(request.Symbol, "5min");

        if (_cache.TryGetValue(fiveMinCacheKey, out TimeSeriesResponse fiveMinData))
        {
            var resampledFromCache = TimeSeriesResampler.Resample(request.Interval, fiveMinData);

            _cache.Set(requestedIntervalKey, resampledFromCache, ttl);

            return MarketDataMapper.MapToClientResponse(resampledFromCache);
        }

        var url = BuildTimeSeriesUrl(request);
        Console.WriteLine(url);

        var response = await SendRequestAsync(url);
        var rawFiveMinData = JsonUtils.ParseResponse<TimeSeriesResponse>(response);

        //Set the raw 5minutes
        _cache.Set(fiveMinCacheKey, rawFiveMinData, ttl);

        TimeSeriesResponse dataToReturn = TimeSeriesResampler.Resample(request.Interval, rawFiveMinData);
        //Set the normal cache
        _cache.Set(requestedIntervalKey, dataToReturn, ttl);

        return MarketDataMapper.MapToClientResponse(dataToReturn);
    }

    private string BuildTimeSeriesUrl(MarketDataDto request)
    {
        var (startDate, endDate) = DateTimeUtils.GenerateRandomMonth();
        var queryParams = new Dictionary<string, string?>
        {
            ["symbol"] = request.Symbol,
            ["interval"] = "5min",
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