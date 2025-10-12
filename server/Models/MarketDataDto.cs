namespace server.Models;

public class MarketDataDto
{
    public string Symbol { get; set; }
    public string Interval { get; set; }
    public long? LastDate { get; set; }
}