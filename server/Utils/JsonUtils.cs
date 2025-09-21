using System.Text.Json;
using System.Text.Json.Serialization;
using server.Models;

namespace server.Utils;

public class JsonUtils
{
    public static T ParseResponse<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentNullException(nameof(json), "Twelve Data returned an empty response.");

        if (json.Contains("\"status\":\"error\"", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Twelve Data API returned an error: " + json);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        try
        {
            var result = JsonSerializer.Deserialize<T>(json, options);

            if (result == null)
                throw new Exception("Failed to deserialize Twelve Data response.");
            
            if (result is TimeSeriesResponse dto && (dto.Values == null || dto.Values.Count == 0))
                throw new Exception("Twelve Data returned no values.");

            return result;
        }
        catch (JsonException ex)
        {
            throw new Exception("Invalid JSON returned from Twelve Data.", ex);
        }
    }
}