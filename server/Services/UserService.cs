using server.Data;
using server.Models;

namespace server.Services;

public class UserService(AppDbContext db) : IUserService
{
    public async Task<CurrentUserDto> GetUser(string userId)
    {
        if (!Guid.TryParse(userId, out Guid userGuid))
            throw new ArgumentException("Invalid user ID format", nameof(userId));

        var user = await db.Users.FindAsync(Guid.Parse(userId));

        if (user == null)
            throw new KeyNotFoundException("User not found");

        return new CurrentUserDto
        {
            Id = user.Id.ToString(),
            Email = user.Email,
        };
    }
}