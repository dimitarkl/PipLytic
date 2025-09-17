using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace server.Models
{
    public class TimeSeriesResponseDto
    {
        [JsonPropertyName("meta")]
        public TimeSeriesMeta Meta { get; set; }

        [JsonPropertyName("values")]
        public List<TimeSeriesValue> Values { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class TimeSeriesMeta
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("interval")]
        public string Interval { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("exchange_timezone")]
        public string ExchangeTimezone { get; set; }
    }

    public class TimeSeriesValue
    {
        [JsonPropertyName("datetime")]
        public string Datetime { get; set; }

        [JsonPropertyName("open")]
        public decimal Open { get; set; }

        [JsonPropertyName("high")]
        public decimal High { get; set; }

        [JsonPropertyName("low")]
        public decimal Low { get; set; }

        [JsonPropertyName("close")]
        public decimal Close { get; set; }

        [JsonPropertyName("volume")]
        public long Volume { get; set; }
    }
}