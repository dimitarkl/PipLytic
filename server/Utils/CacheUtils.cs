namespace server.Utils;

public static class CacheUtils
{
    public static string GenerateMarketDataCacheKey(string symbol, string startDate)
    {
        var yearMonth =  ParseYearMonth(startDate);
        return $"stock:data:{symbol}:{yearMonth}";
    }

    public static string GenerateUserCacheKey(string userId, string symbol, string currentBaseCache)
    {
        return $"user:{userId}:{currentBaseCache}";
    }
    
    private static string ParseYearMonth(string startDate)
    {
        if (DateTime.TryParse(startDate, out var date))
            return date.ToString("yyyy-MM");
        
        return startDate.Length >= 7 ? startDate[..7] : startDate;
    }

    public static TimeSpan MarketDataTtl => TimeSpan.FromMinutes(3);

    public static TimeSpan UserCacheSlidingTtl => TimeSpan.FromMinutes(5);
}