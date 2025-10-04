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

    public EUserType EUserType { get; set; } = EUserType.Free;
}