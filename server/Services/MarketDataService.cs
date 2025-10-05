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

    public TimeSeriesResponse? GetUserCacheIfExists(string userId, string symbol)
    {
        var userCacheKey = $"user:{userId}:symbol:{symbol}:5min";
        var userTtl = CacheUtils.UserCacheSlidingTtl;

        if (_cache.TryGetValue(userCacheKey, out TimeSeriesResponse cachedData))
        {
            _cache.Set(userCacheKey, cachedData, userTtl);
            return cachedData;
        }

        return null;
    }

    private async Task<TimeSeriesResponse> GetSharedFiveMinCacheAsync(string symbol)
    {
        var cacheKey = CacheUtils.GenerateMarketDataCacheKey(symbol, "5min");
        var sharedTtl = CacheUtils.MarketDataTtl;

        if (_cache.TryGetValue(cacheKey, out TimeSeriesResponse cachedData))
            return cachedData;


        var url = BuildTimeSeriesUrl(new MarketDataDto { Symbol = symbol, Interval = "5min" });

        var response = await SendRequestAsync(url);
        var data = JsonUtils.ParseResponse<TimeSeriesResponse>(response);

        _cache.Set(cacheKey, data, sharedTtl);

        return data;
    }

    private void StoreUserCache(string userId, string symbol, TimeSeriesResponse baseData)
    {
        var userCacheKey = $"user:{userId}:symbol:{symbol}:5min";
        var userTtl = CacheUtils.UserCacheSlidingTtl;

        var userDataCopy = baseData.DeepClone();
        _cache.Set(userCacheKey, userDataCopy, userTtl);
    }

    public async Task<StocksSearchResponseDto> QueryStocksData(MarketDataDto request, string userId)
    {
        var userCache = GetUserCacheIfExists(userId, request.Symbol);
        if (userCache != null)
        {
            var resampled = TimeSeriesResampler.Resample(request.Interval, userCache);
            return MarketDataMapper.MapToClientResponse(resampled);
        }

        var sharedBase = await GetSharedFiveMinCacheAsync(request.Symbol);

        StoreUserCache(userId, request.Symbol, sharedBase);

        var userCacheForRequest = GetUserCacheIfExists(userId, request.Symbol);
        var resampledData = TimeSeriesResampler.Resample(request.Interval, userCacheForRequest);

        return MarketDataMapper.MapToClientResponse(resampledData);
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

        return !response.IsSuccessStatusCode
            ? throw new HttpRequestException($"API request failed ({response.StatusCode}): {content}")
            : content;
    }
}