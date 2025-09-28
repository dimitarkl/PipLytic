using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Entities;

[Table("trades")]
public class Trade
{
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("user_id")]
    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } 
    
    [Column("amount_invested")]
    [Required]
    public decimal AmountInvested { get; set; }
    
    [Column("type")]
    [Required]
    public string Type { get; set; }
    
    [Column("ticker")]
    [Required]
    public string Symbol { get; set; }
    
    [Column("amount_final")]
    public decimal AmountFinal { get; set; }
    
    [Column("start_date")]
    [Required]
    public long StartDate { get; set; }
    
    [Column("end_date")]
    public long EndDate { get; set; }
    

}