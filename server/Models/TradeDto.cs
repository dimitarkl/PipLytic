using Microsoft.Build.Framework;

namespace server.Models;

public class StartTradeDto
{
    [Required] public decimal AmountInvested { get; set; }
    
    public string Type { get; set; }

    public string Symbol { get; set; }
}

public class EndTradeDto
{
    public string TradeId { get; set; }
    public decimal AmountFinal { get; set; }
}