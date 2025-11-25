using PipLytic.Api.Models;
using PipLytic.Api.Entities;

namespace PipLytic.Api.Services;

public interface IAuthService
{
    Task<TokenResponseDto> RegisterAsync(UserDto request);
    Task<TokenResponseDto> LoginAsync(UserDto request);
    Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken);
    CookieOptions CreateCookieOptions();
}