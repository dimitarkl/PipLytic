using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Entities;

[Table("users")]
public class User
{
    [Column("id")] public Guid Id { get; set; }

    [Column("email")]
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = "";

    [Column("password_hash")]
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = "";

    [Column("refresh_token")]
    [MaxLength(512)]
    public string? RefreshToken { get; set; }

    [Column("refresh_token_expiry")]
    public DateTime? RefreshTokenExpiry { get; set; }

    public ICollection<Trade> Trades { get; set; }
}