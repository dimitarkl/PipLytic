using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        return CreateTokenResponse(user);
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


        return CreateTokenResponse(user);
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var userId = ValidateRefreshToken(refreshToken);

        if (userId == null)
            return null;

        var user = await _db.Users.FindAsync(userId);
        
        if (user is null)
            return null;

        return CreateTokenResponse(user);
    }

    private TokenResponseDto CreateTokenResponse(User user)
    {
        return new TokenResponseDto
        {
            AccessToken = CreateToken(user),
            RefreshToken = GenerateRefreshToken(user)
        };
    }

    private Guid? ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!);
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _configuration.GetValue<string>("AppSettings:Issuer"),
                ValidateAudience = true,
                ValidAudience = _configuration.GetValue<string>("AppSettings:Audience"),
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out var validatedToken);
            
            if (validatedToken is not JwtSecurityToken jwtToken || 
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            return userId;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateRefreshToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("token_type", "refresh")
        };
        
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
            audience: _configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
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