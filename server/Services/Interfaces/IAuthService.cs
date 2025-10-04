using server.Entities;
using server.Models;

namespace server.Services;

public interface IAuthService
{
    Task<TokenResponseDto> RegisterAsync(UserDto request);
    Task<TokenResponseDto> LoginAsync(UserDto request);
    Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken);
    CookieOptions CreateCookieOptions();
}