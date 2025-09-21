using System;
using System.Collections.Generic;

namespace server.Models
{
    // Models for sending to client
    public class StocksSearchResponseDto
    {
        public StocksSearchResponseMeta Meta { get; set; }
        public List<StocksSearchResponseValue> Values { get; set; }
        public string Status { get; set; }
    }

    public class StocksSearchResponseMeta
    {
        public string Symbol { get; set; }
        public string Interval { get; set; }
        public string Currency { get; set; }
        public string ExchangeTimezone { get; set; }
    }

    public class StocksSearchResponseValue
    {
        public long Time { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }
}