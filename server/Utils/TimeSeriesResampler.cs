using server.Models;

namespace server.Utils;

public class TimeSeriesResampler
{
    public static TimeSeriesResponse Resample(string targetInterval, TimeSeriesResponse timeSeriesJson)
    {
        TimeSeriesResponse resampledJson;
        var timeSeriesValues = timeSeriesJson.Values;
        switch (targetInterval)
        {
            case "5min":
                resampledJson = timeSeriesJson;
                break;
            case "15min":
                resampledJson = ResampleTimeSeries(timeSeriesJson, 3);
                break;
            case "1h":
                resampledJson = ResampleTimeSeries(timeSeriesJson, 12);//12 * 5 = 60
                break;
            default:
                throw new Exception($"Invalid target interval: {targetInterval}");
        }

        return resampledJson;
    }

    private static TimeSeriesResponse ResampleTimeSeries(
        TimeSeriesResponse timeSeriesJson,
        int groupSize)
    {
        var resampledJson = new TimeSeriesResponse
        {
            Meta = timeSeriesJson.Meta,
            Status = timeSeriesJson.Status,
            Values = new List<TimeSeriesResponseValue>()
        };
        var timeSeriesValues = timeSeriesJson.Values;

        for (int i = 0; i + groupSize <= timeSeriesValues.Count; i += groupSize)
        {
            var values = timeSeriesValues.GetRange(i, groupSize);
            
            //TODO Risky if data  changes
            resampledJson.Values.Add(new TimeSeriesResponseValue
            {
                DateTimeRaw = values[^1].DateTimeRaw,
                Open = values[^1].Open,
                High = values.Max(v => v.High),
                Low = values.Min(v => v.Low),
                Close = values[0].Close,
                Volume = values.Sum(v => v.Volume)
            });
        }

        return resampledJson;
    }
}