namespace server.Utils;

public static class CacheUtils
{

    public static string GenerateMarketDataCacheKey(string symbol, string startDate)
    {
        var yearMonth = startDate.Substring(0, 7);
        return $"{symbol}:{yearMonth}";
    }

    public static string GenerateUserCacheKey(string userId, string symbol)
    {
        return $"user:{userId}:symbol:{symbol}";
    }

    public static TimeSpan MarketDataTtl => TimeSpan.FromMinutes(3);
    
    public static TimeSpan UserCacheSlidingTtl => TimeSpan.FromMinutes(5);

}