using PipLytic.Api.Data;
using PipLytic.Api.Exceptions;
using PipLytic.Api.Models;

namespace PipLytic.Api.Services;

public class UserService(AppDbContext db) : IUserService
{
    public async Task<CurrentUserDto> GetUser(Guid userId)
    {
        var user = await db.Users.FindAsync(userId);

        if (user == null)
            throw new NotFoundException("User not found");

        return new CurrentUserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserType = user.UserType,
        };
    }
}