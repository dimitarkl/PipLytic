using System.Collections.Concurrent;
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


    private static readonly ConcurrentDictionary<string, string> _symbolToCacheKey = new();

    private const string DefaultInterval = "5min";
    private const string ApiKeyConfigPath = "AppSettings:TwelveDataApiKey";

    public MarketDataService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IMemoryCache cache)
    {
        _client = httpClientFactory.CreateClient("TwelveData");
        _configuration = configuration;
        _cache = cache;
    }

    public TimeSeriesResponse? GetUserCacheIfExists(string userId, string symbol)
    {
        var userCacheKey = CacheUtils.GenerateUserCacheKey(userId, symbol);
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
        if (_symbolToCacheKey.TryGetValue(symbol, out var existingCacheKey))
        {
            if (_cache.TryGetValue(existingCacheKey, out TimeSeriesResponse cachedData))
            {
                return cachedData;
            }

            _symbolToCacheKey.TryRemove(symbol, out _);
        }

        var (startDate, endDate) = DateTimeUtils.GenerateRandomMonth();
        var cacheKey = CacheUtils.GenerateMarketDataCacheKey(symbol, startDate);
        var sharedTtl = CacheUtils.MarketDataTtl;

        var url = BuildTimeSeriesUrl(new MarketDataDto { Symbol = symbol, Interval = DefaultInterval },
            (startDate, endDate));

        var response = await SendRequestAsync(url);
        var data = JsonUtils.ParseResponse<TimeSeriesResponse>(response);

        _cache.Set(cacheKey, data, sharedTtl);

        _symbolToCacheKey[symbol] = cacheKey;

        return data;
    }

    private void StoreUserCache(string userId, string symbol, TimeSeriesResponse baseData)
    {
        var userCacheKey = CacheUtils.GenerateUserCacheKey(userId, symbol);
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

        var resampledData = TimeSeriesResampler.Resample(request.Interval, sharedBase);

        return MarketDataMapper.MapToClientResponse(resampledData);
    }

    public async Task<StocksSearchResponseDto> ContinueStocksData(MarketDataDto request, string userId)
    {
        var userCacheKey = CacheUtils.GenerateUserCacheKey(userId, request.Symbol);
        _cache.Remove(userCacheKey);

        if (request.LastDate == null) throw new ArgumentException("LastDate cannot be null.");

        var nextMonth = DateTimeOffset.FromUnixTimeSeconds(request.LastDate.Value).UtcDateTime.AddMonths(1);
        var url = BuildTimeSeriesUrl(request, DateTimeUtils.GetMonthDateRange(nextMonth));
        var response = await SendRequestAsync(url);
        var data = JsonUtils.ParseResponse<TimeSeriesResponse>(response);

        StoreUserCache(userId, request.Symbol, data);

        var resampledData = TimeSeriesResampler.Resample(request.Interval, data);

        return MarketDataMapper.MapToClientResponse(resampledData);
    }

    public async Task<StocksSearchResponseDto> RefreshStocksData(MarketDataDto request, string userId)
    {
        var userCacheKey = CacheUtils.GenerateUserCacheKey(userId, request.Symbol);
        
        //Clear User & Symbol Cache
        //Deletes the cache for everyone(in the future could be fixed/or not)
        _cache.Remove(userCacheKey);
        _symbolToCacheKey.TryRemove(request.Symbol, out _);
        
        var sharedBase = await GetSharedFiveMinCacheAsync(request.Symbol);

        StoreUserCache(userId, request.Symbol, sharedBase);

        var resampledData = TimeSeriesResampler.Resample(request.Interval, sharedBase);

        return MarketDataMapper.MapToClientResponse(resampledData);
    }


    private string BuildTimeSeriesUrl(MarketDataDto request, (string startDate, string endDate)? range = null)
    {
        var (startDate, endDate) = range ?? DateTimeUtils.GenerateRandomMonth();
        var queryParams = new Dictionary<string, string?>
        {
            ["symbol"] = request.Symbol,
            ["interval"] = DefaultInterval,
            ["apikey"] = _configuration.GetValue<string>(ApiKeyConfigPath),
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