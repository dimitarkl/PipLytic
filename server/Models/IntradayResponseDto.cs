using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace server.Models;

public class IntradayResponseDto
{
    [JsonPropertyName("Meta Data")]
    public MetaData MetaData { get; set; }

    [JsonPropertyName("Time Series (1min)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, TimeSeriesData> TimeSeries1Min { get; set; }

    [JsonPropertyName("Time Series (5min)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, TimeSeriesData> TimeSeries5Min { get; set; }

    [JsonPropertyName("Time Series (15min)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, TimeSeriesData> TimeSeries15Min { get; set; }

    [JsonPropertyName("Time Series (30min)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, TimeSeriesData> TimeSeries30Min { get; set; }

    [JsonPropertyName("Time Series (60min)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, TimeSeriesData> TimeSeries60Min { get; set; }
}

public class MetaData
{
    [JsonPropertyName("1. Information")]
    public string Information { get; set; }

    [JsonPropertyName("2. Symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("3. Last Refreshed")]
    public string LastRefreshed { get; set; }

    [JsonPropertyName("4. Interval")]
    public string Interval { get; set; }

    [JsonPropertyName("5. Output Size")]
    public string OutputSize { get; set; }

    [JsonPropertyName("6. Time Zone")]
    public string TimeZone { get; set; }
}

public class TimeSeriesData
{
    [JsonPropertyName("1. open")]
    public decimal Open { get; set; }

    [JsonPropertyName("2. high")]
    public decimal High { get; set; }

    [JsonPropertyName("3. low")]
    public decimal Low { get; set; }

    [JsonPropertyName("4. close")]
    public decimal Close { get; set; }

    [JsonPropertyName("5. volume")]
    public long Volume { get; set; }
}
