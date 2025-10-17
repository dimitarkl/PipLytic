using Microsoft.Build.Framework;

namespace server.Models;

public class StartTradeDto
{
    [Required] public decimal AmountInvested { get; set; }
    
    [Required] public string Type { get; set; }

    [Required] public string Symbol { get; set; }
}

public class EndTradeDto
{
    public string TradeId { get; set; }
    public decimal AmountFinal { get; set; }
}