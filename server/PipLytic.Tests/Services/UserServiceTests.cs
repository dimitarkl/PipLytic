using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PipLytic.Api.Data;
using PipLytic.Api.Entities;
using PipLytic.Api.Exceptions;
using PipLytic.Api.Services;

namespace PipLytic.Tests.Services;

public class UserServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _sut = new UserService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetUser_WithExistingUser_ReturnsCorrectDto()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "hash" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var result = await _sut.GetUser(user.Id);

        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.UserType.Should().Be(user.UserType);
    }

    [Fact]
    public async Task GetUser_WithNonexistentId_ThrowsNotFoundException()
    {
        var act = async () => await _sut.GetUser(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("User not found");
    }
}
