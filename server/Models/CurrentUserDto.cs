using server.Enums;

namespace server.Models;

public class CurrentUserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public EUserType UserType { get; set; }
}