using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using server.Data;
using server.Entities;
using server.Models;
using server.Exceptions;

namespace server.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private IAuthService _authServiceImplementation;

    public AuthService(AppDbContext db, IConfiguration configuration)
    {
        _configuration = configuration;
        _db = db;
    }

    public async Task<TokenResponseDto> RegisterAsync(UserDto request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new UserAlreadyExistsException($"User already exists");


        var user = new User();

        var hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, request.Password);

        user.Email = request.Email;
        user.PasswordHash = hashedPassword;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return await CreateTokenResponse(user);
    }

    public async Task<TokenResponseDto> LoginAsync(UserDto request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            throw new InvalidCredentialsException("Invalid email or password");

        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialsException("Invalid email or password");
        }


        return await CreateTokenResponse(user);
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var user = await ValidateRefreshTokenAsync(request.RefreshToken);

        if (user is null)
            return null;

        return await CreateTokenResponse(user);
    }

    private async Task<TokenResponseDto> CreateTokenResponse(User? user)
    {
        return new TokenResponseDto
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateRefreshTokenAsync(user)
        };
    }

    private async Task<User?> ValidateRefreshTokenAsync(string refreshToken)
    {
        //TODO SEND Exception
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
            return null;

        return user;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateRefreshTokenAsync(User user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();
        return refreshToken;
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
            audience: _configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );


        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public CookieOptions CreateCookieOptions()
    {
        //TODO Write Logic For deciding Secure
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(30),
            Path = "/"
        };
    }
}