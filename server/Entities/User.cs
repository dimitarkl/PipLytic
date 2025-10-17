using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using server.Enums;

namespace server.Entities;

[Table("users")]
public class User
{
    [Column("id")] public Guid Id { get; set; }

    [Column("email")]
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = "";

    [Column("password_hash")]
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = "";

    public ICollection<Trade> Trades { get; set; }

    [Column("ai_uses_left")] 
    public int AiUsesLeft{ get; private set; } = 5;
    
    [Column("ai_reset_at")]
    public DateTime AiResetAt { get; private set; } = DateTime.UtcNow;

    [Column("user_type")]
    public EUserType UserType { get; set; } = EUserType.Free;

    public bool TryUseAi()
    {
        if (DateTime.UtcNow >= AiResetAt)
        {
            AiUsesLeft = 5; 
            AiResetAt = DateTime.UtcNow.AddHours(6);
        }

        if (AiUsesLeft > 0)
        {
            AiUsesLeft--;
            return true;
        }

        return false; 
    }
}


