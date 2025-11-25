namespace PipLytic.Api.Models;

public class ChatResponse
{
    public string Role { get; set; }
    public string Message { get; set; }
    
    public int Index { get; set; }
}