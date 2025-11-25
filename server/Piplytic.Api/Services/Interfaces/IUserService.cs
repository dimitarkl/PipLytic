using PipLytic.Api.Models;

namespace PipLytic.Api.Services;

public interface IUserService
{
    Task<CurrentUserDto> GetUser(Guid userId);
}