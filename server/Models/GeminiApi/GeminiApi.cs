using System.Text.Json.Serialization;

namespace server.Models.GeminiApi;

public class GeminiRequestPayload
{
    [JsonPropertyName("system_instruction")]
    public required SystemInstruction SystemInstruction { get; set; }
    
    [JsonPropertyName("contents")]
    public required List<Content> Contents { get; set; }
}

public class SystemInstruction
{
    [JsonPropertyName("parts")]
    public required List<Part> Parts { get; set; }
}

public class Content
{
    [JsonPropertyName("role")]
    public required string Role { get; set; }
    
    [JsonPropertyName("parts")]
    public required List<Part> Parts { get; set; }
}

public class Part
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}