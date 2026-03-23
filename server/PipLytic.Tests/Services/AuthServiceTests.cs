using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PipLytic.Api.Data;
using PipLytic.Api.Exceptions;
using PipLytic.Api.Models;
using PipLytic.Api.Services;

namespace PipLytic.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuthService _sut;
    private readonly IWebHostEnvironment _env;

    private const string AccessSecret = "test-access-token-secret-that-is-long-enough-for-hmac-sha512-hashing!!";
    private const string RefreshSecret = "test-refresh-token-secret-that-is-long-enough-for-hmac-sha512-hashing!";
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:AccessTokenSecret"] = AccessSecret,
                ["AppSettings:RefreshTokenSecret"] = RefreshSecret,
                ["AppSettings:Issuer"] = Issuer,
                ["AppSettings:Audience"] = Audience,
            })
            .Build();

        _env = A.Fake<IWebHostEnvironment>();
        A.CallTo(() => _env.EnvironmentName).Returns(Environments.Development);

        _sut = new AuthService(_db, config, _env);
    }

    public void Dispose() => _db.Dispose();

    // --- RegisterAsync ---

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ReturnsTokenResponse()
    {
        var request = new UserDto { Email = "new@example.com", Password = "password123" };

        var result = await _sut.RegisterAsync(request);

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_PersistsUser()
    {
        var request = new UserDto { Email = "new@example.com", Password = "password123" };

        await _sut.RegisterAsync(request);

        var saved = await _db.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsUserAlreadyExistsException()
    {
        var request = new UserDto { Email = "existing@example.com", Password = "password123" };
        await _sut.RegisterAsync(request);

        var act = async () => await _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<UserAlreadyExistsException>();
    }

    // --- LoginAsync ---

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenResponse()
    {
        var request = new UserDto { Email = "user@example.com", Password = "securePass1" };
        await _sut.RegisterAsync(request);

        var result = await _sut.LoginAsync(request);

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsInvalidCredentialsException()
    {
        var request = new UserDto { Email = "nobody@example.com", Password = "password" };

        var act = async () => await _sut.LoginAsync(request);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsInvalidCredentialsException()
    {
        var request = new UserDto { Email = "user@example.com", Password = "correctPassword" };
        await _sut.RegisterAsync(request);

        var act = async () => await _sut.LoginAsync(new UserDto { Email = request.Email, Password = "wrongPassword" });

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    // --- RefreshTokenAsync ---

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokenResponse()
    {
        var request = new UserDto { Email = "user@example.com", Password = "password" };
        var tokens = await _sut.RegisterAsync(request);

        var result = await _sut.RefreshTokenAsync(tokens.RefreshToken);

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithGarbageToken_ReturnsNull()
    {
        var result = await _sut.RefreshTokenAsync("this.is.not.a.real.token");

        result.Should().BeNull();
    }

    // --- CreateCookieOptions ---

    [Fact]
    public void CreateCookieOptions_InDevelopment_SecureIsFalse()
    {
        A.CallTo(() => _env.EnvironmentName).Returns(Environments.Development);

        var options = _sut.CreateCookieOptions();

        options.Secure.Should().BeFalse();
        options.HttpOnly.Should().BeTrue();
    }

    [Fact]
    public void CreateCookieOptions_InProduction_SecureIsTrue()
    {
        A.CallTo(() => _env.EnvironmentName).Returns(Environments.Production);

        var options = _sut.CreateCookieOptions();

        options.Secure.Should().BeTrue();
    }
}
