using server.Models;

namespace server.Services;

public interface IMarketDataService
{
    Task<TimeSeriesResponseDto> QueryStocksData(MarketDataDto request);
}