using System.Collections.Concurrent;
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

    private static readonly ConcurrentDictionary<string, string> SymbolToCacheKey = new();
    
    // "userId:symbol" -> "user:userId:stock:data:SYMBOL:YYYY-MM"
    private static readonly ConcurrentDictionary<string, string> UserCacheKeys = new();

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
        var userCacheKeyLookup = $"{userId}:{symbol}";
        
        if (!UserCacheKeys.TryGetValue(userCacheKeyLookup, out var userCacheKey))
        {
            return null; 
        }

        var userTtl = CacheUtils.UserCacheSlidingTtl;

        if (_cache.TryGetValue(userCacheKey, out TimeSeriesResponse? cachedData))
        {
            _cache.Set(userCacheKey, cachedData, userTtl);
            return cachedData;
        }
        
        UserCacheKeys.TryRemove(userCacheKeyLookup, out _);
        return null;
    }

    private async Task<TimeSeriesResponse> GetSharedFiveMinCacheAsync(string symbol)
    {
        if (SymbolToCacheKey.TryGetValue(symbol, out var existingCacheKey))
        {
            if (_cache.TryGetValue(existingCacheKey, out TimeSeriesResponse cachedData))
            {
                return cachedData;
            }

            SymbolToCacheKey.TryRemove(symbol, out _);
        }

        var (startDate, endDate) = DateTimeUtils.GenerateRandomMonth();
        var cacheKey = CacheUtils.GenerateMarketDataCacheKey(symbol, startDate);
        var sharedTtl = CacheUtils.MarketDataTtl;

        var url = BuildTimeSeriesUrl(new MarketDataDto { Symbol = symbol, Interval = DefaultInterval },
            (startDate, endDate));

        var response = await SendRequestAsync(url);
        var data = JsonUtils.ParseResponse<TimeSeriesResponse>(response);

        // Create cache entry options with eviction callback
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(sharedTtl)
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                // Clean up the symbol mapping when cache expires
                var symbolToClean = state as string;
                if (symbolToClean != null && SymbolToCacheKey.TryGetValue(symbolToClean, out var mappedKey))
                {
                    if (mappedKey == key.ToString())
                    {
                        SymbolToCacheKey.TryRemove(symbolToClean, out _);
                    }
                }
            }, symbol);

        _cache.Set(cacheKey, data, cacheEntryOptions);

        SymbolToCacheKey[symbol] = cacheKey;

        return data;
    }

    private void StoreUserCache(string userId, string symbol, TimeSeriesResponse baseData)
    {

        if (!SymbolToCacheKey.TryGetValue(symbol, out var sharedCacheKey))
            return;

        if (!_cache.TryGetValue(sharedCacheKey, out TimeSeriesResponse? _))
        {
            SymbolToCacheKey.TryRemove(symbol, out _);
            return;
        }
        
        var userCacheKey = CacheUtils.GenerateUserCacheKey(userId, symbol, sharedCacheKey);
        var userTtl = CacheUtils.UserCacheSlidingTtl;

        var userDataCopy = baseData.DeepClone();
        
        var userCacheKeyLookup = $"{userId}:{symbol}";
        
        // Create cache entry options with eviction callback
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(userTtl)
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                var lookupKey = state as string;
                if (lookupKey != null && UserCacheKeys.TryGetValue(lookupKey, out var mappedKey))
                {
                    if (mappedKey == key.ToString())
                    {
                        UserCacheKeys.TryRemove(lookupKey, out _);
                    }
                }
            }, userCacheKeyLookup);
        
        _cache.Set(userCacheKey, userDataCopy, cacheEntryOptions);
        
        UserCacheKeys[userCacheKeyLookup] = userCacheKey;
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
        // Remove old user cache if exists
        if (SymbolToCacheKey.TryGetValue(request.Symbol, out var oldSharedCacheKey))
        {
            var oldUserCacheKey = CacheUtils.GenerateUserCacheKey(userId, request.Symbol, oldSharedCacheKey);
            _cache.Remove(oldUserCacheKey);
            
            // Clean up user cache mapping
            var userCacheKeyLookup = $"{userId}:{request.Symbol}";
            UserCacheKeys.TryRemove(userCacheKeyLookup, out _);
        }

        if (request.LastDate == null) throw new ArgumentException("LastDate cannot be null.");

        var nextMonth = DateTimeOffset.FromUnixTimeSeconds(request.LastDate.Value).UtcDateTime.AddMonths(1);
        var (startDate, endDate) = DateTimeUtils.GetMonthDateRange(nextMonth);
        
        var newCacheKey = CacheUtils.GenerateMarketDataCacheKey(request.Symbol, startDate);
        var sharedTtl = CacheUtils.MarketDataTtl;
        
        var url = BuildTimeSeriesUrl(request, (startDate, endDate));
        var response = await SendRequestAsync(url);
        var data = JsonUtils.ParseResponse<TimeSeriesResponse>(response);
        
        // Create cache entry options with eviction callback
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(sharedTtl)
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                // Clean up the symbol mapping when cache expires
                var symbolToClean = state as string;
                if (symbolToClean != null && SymbolToCacheKey.TryGetValue(symbolToClean, out var mappedKey))
                {
                    if (mappedKey == key.ToString())
                    {
                        SymbolToCacheKey.TryRemove(symbolToClean, out _);
                    }
                }
            }, request.Symbol);
        
        _cache.Set(newCacheKey, data, cacheEntryOptions);
        SymbolToCacheKey[request.Symbol] = newCacheKey;
        
        StoreUserCache(userId, request.Symbol, data);

        var resampledData = TimeSeriesResampler.Resample(request.Interval, data);

        return MarketDataMapper.MapToClientResponse(resampledData);
    }

    public async Task<StocksSearchResponseDto> RefreshStocksData(MarketDataDto request, string userId)
    {
        // Remove user cache if exists
        var userCacheKeyLookup = $"{userId}:{request.Symbol}";
        if (UserCacheKeys.TryGetValue(userCacheKeyLookup, out var oldUserCacheKey))
        {
            _cache.Remove(oldUserCacheKey);
            UserCacheKeys.TryRemove(userCacheKeyLookup, out _);
            
            if (SymbolToCacheKey.TryGetValue(request.Symbol, out var currentSharedCacheKey))
            {
                var userCacheKeyWithoutUserPrefix = oldUserCacheKey.Replace($"user:{userId}:", "");

                if (currentSharedCacheKey == userCacheKeyWithoutUserPrefix)
                    SymbolToCacheKey.TryRemove(request.Symbol, out _);
                
            }
        }

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