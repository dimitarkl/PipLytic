using System.ComponentModel.DataAnnotations;

namespace server.Entities;

public class User
{
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = "";

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = "";

    [MaxLength(512)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }
}