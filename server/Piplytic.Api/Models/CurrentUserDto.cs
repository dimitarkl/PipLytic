using PipLytic.Api.Enums;

namespace PipLytic.Api.Models;

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public EUserType UserType { get; set; }
}