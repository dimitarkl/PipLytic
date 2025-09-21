using server.Models;

namespace server.Services;

public interface IMarketDataService
{
    Task<StocksSearchResponseDto> QueryStocksData(MarketDataDto request);
}