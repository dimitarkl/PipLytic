using server.Models;

namespace server.Services;

public interface IMarketDataService
{
    Task<StocksSearchResponseDto> QueryStocksData(MarketDataDto request,string userId);

    Task<StocksSearchResponseDto> ContinueStocksData(MarketDataDto request,string userId);

    Task<StocksSearchResponseDto> RefreshStocksData(MarketDataDto request,string userId);

    TimeSeriesResponse? GetUserCacheIfExists(string userId, string symbol);
    
}