using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Entities;

[Table("trades")]
public class Trade
{
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("user_id")]
    public Guid UserId { get; set; }
    public User User { get; set; } 
    
    [Column("amount_invested")]
    public decimal AmountInvested { get; set; }
    
    [Column("amount_final")]
    public decimal AmountFinal { get; set; }
    
    [Column("executed_at")]
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}