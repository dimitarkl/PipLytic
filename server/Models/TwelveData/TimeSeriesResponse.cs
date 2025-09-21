using System.Text.Json.Serialization;
using server.Utils;

namespace server.Models
{
    public class TimeSeriesResponse
    {
        [JsonPropertyName("meta")]
        public TimeSeriesResponseMeta Meta { get; set; }

        [JsonPropertyName("values")]
        public List<TimeSeriesResponseValue> Values { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class TimeSeriesResponseMeta
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

    public class TimeSeriesResponseValue
    {
        private string _dateTimeString;

        [JsonPropertyName("datetime")]
        public string DateTimeRaw
        {
            get => _dateTimeString;
            set => _dateTimeString = value;
        }
        [JsonIgnore] 
        public long DateTime => DateTimeUtils.ToUnixTimestamp(_dateTimeString);

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
