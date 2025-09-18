using server.Models;

namespace server.Services;

public interface IUserService
{
    Task<CurrentUserDto> GetUser(string userId);
}