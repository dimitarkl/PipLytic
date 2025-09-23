using server.Models;

namespace server.Services.Mappers
{
    public static class MarketDataMapper
    {
        public static StocksSearchResponseDto MapToClientResponse(TimeSeriesResponse timeSeriesResponse)
        {
            return new StocksSearchResponseDto
            {
                Meta = new StocksSearchResponseMeta
                {
                    Symbol = timeSeriesResponse.Meta.Symbol,
                    Interval = timeSeriesResponse.Meta.Interval,
                    Currency = timeSeriesResponse.Meta.Currency,
                    ExchangeTimezone = timeSeriesResponse.Meta.ExchangeTimezone
                },
                Values = timeSeriesResponse.Values.Select(v => new StocksSearchResponseValue
                {
                    Time = v.DateTime,
                    Open = v.Open,
                    High = v.High,
                    Low = v.Low,
                    Close = v.Close,
                    Volume = v.Volume
                }).OrderBy(v => v.Time).ToList(),
                Status = timeSeriesResponse.Status
            };
        }
    }
}