using server.Models;

namespace server.Services;

public interface IMarketDataService
{
    Task<IntradayResponseDto> QueryStocksData(MarketDataDto request);
}