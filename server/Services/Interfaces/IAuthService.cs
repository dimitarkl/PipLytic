using server.Entities;
using server.Models;

namespace server.Services;

public interface IAuthService
{
    Task<TokenResponseDto> RegisterAsync(UserDto request);
    Task<TokenResponseDto> LoginAsync(UserDto request);
    Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
    CookieOptions CreateCookieOptions();
}