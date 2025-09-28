namespace server.Utils;

public static class CacheUtils
{

    public static string GenerateMarketDataCacheKey(string symbol, string interval)
    {
        return $"{symbol}:{interval}";
    }

    public static TimeSpan MarketDataTtl => TimeSpan.FromMinutes(3);
    
    public static TimeSpan UserCacheSlidingTtl => TimeSpan.FromMinutes(5);

}