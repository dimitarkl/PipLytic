namespace server.Models;

public class ChatRequest
{
    public string Message { get; set; }
    public string Symbol { get; set; }
    public long EndDate { get; set; }
}