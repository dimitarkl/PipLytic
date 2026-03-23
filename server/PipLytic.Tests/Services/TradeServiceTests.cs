using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PipLytic.Api.Data;
using PipLytic.Api.Entities;
using PipLytic.Api.Exceptions;
using PipLytic.Api.Models;
using PipLytic.Api.Services;

namespace PipLytic.Tests.Services;

public class TradeServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly TradeService _sut;

    public TradeServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _sut = new TradeService(_db);
    }

    public void Dispose() => _db.Dispose();

    // --- AddTrade ---

    [Fact]
    public async Task AddTrade_WithLongType_ReturnsTradeWithCorrectProperties()
    {
        var userId = Guid.NewGuid();
        var request = new StartTradeDto { AmountInvested = 1000m, Type = "long", Symbol = "AAPL" };

        var result = await _sut.AddTrade(userId, request);

        result.UserId.Should().Be(userId);
        result.AmountInvested.Should().Be(1000m);
        result.Type.Should().Be("long");
        result.Symbol.Should().Be("AAPL");
        result.StartDate.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AddTrade_WithShortType_ReturnsTrade()
    {
        var userId = Guid.NewGuid();
        var request = new StartTradeDto { AmountInvested = 500m, Type = "short", Symbol = "NVDA" };

        var result = await _sut.AddTrade(userId, request);

        result.Type.Should().Be("short");
    }

    [Fact]
    public async Task AddTrade_WithInvalidType_ThrowsException()
    {
        var userId = Guid.NewGuid();
        var request = new StartTradeDto { AmountInvested = 1000m, Type = "buy", Symbol = "AAPL" };

        var act = async () => await _sut.AddTrade(userId, request);

        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid trade type");
    }

    [Fact]
    public async Task AddTrade_PersistsTrade()
    {
        var userId = Guid.NewGuid();
        var request = new StartTradeDto { AmountInvested = 1000m, Type = "long", Symbol = "MSFT" };

        var result = await _sut.AddTrade(userId, request);

        var persisted = await _db.Trades.FindAsync(result.Id);
        persisted.Should().NotBeNull();
    }

    // --- EndTrade ---

    [Fact]
    public async Task EndTrade_WithValidRequest_UpdatesAmountFinalAndEndDate()
    {
        var userId = Guid.NewGuid();
        var trade = CreateTrade(userId);
        await SeedAsync(trade);

        var request = new EndTradeDto { TradeId = trade.Id.ToString(), AmountFinal = 1200m };

        var result = await _sut.EndTrade(userId, request);

        result.AmountFinal.Should().Be(1200m);
        result.EndDate.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task EndTrade_WithInvalidGuidFormat_ThrowsException()
    {
        var request = new EndTradeDto { TradeId = "not-a-guid", AmountFinal = 1000m };

        var act = async () => await _sut.EndTrade(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid trade ID format.");
    }

    [Fact]
    public async Task EndTrade_WithNonexistentTrade_ThrowsNotFoundException()
    {
        var request = new EndTradeDto { TradeId = Guid.NewGuid().ToString(), AmountFinal = 1000m };

        var act = async () => await _sut.EndTrade(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Trade not found");
    }

    [Fact]
    public async Task EndTrade_WhenCalledByDifferentUser_ThrowsForbiddenException()
    {
        var ownerId = Guid.NewGuid();
        var trade = CreateTrade(ownerId);
        await SeedAsync(trade);

        var request = new EndTradeDto { TradeId = trade.Id.ToString(), AmountFinal = 1200m };

        var act = async () => await _sut.EndTrade(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // --- GetTrade ---

    [Fact]
    public async Task GetTrade_WithExistingId_ReturnsTrade()
    {
        var trade = CreateTrade(Guid.NewGuid());
        await SeedAsync(trade);

        var result = await _sut.GetTrade(trade.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(trade.Id);
    }

    [Fact]
    public async Task GetTrade_WithNonexistentId_ReturnsNull()
    {
        var result = await _sut.GetTrade(Guid.NewGuid());

        result.Should().BeNull();
    }

    // --- GetUserTrades ---

    [Fact]
    public async Task GetUserTrades_ReturnsOnlyTradesForThatUser()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        await SeedAsync(
            CreateTrade(userId, executedAt: 1000),
            CreateTrade(otherUserId, executedAt: 2000)
        );

        var result = await _sut.GetUserTrades(userId);

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetUserTrades_ReturnsTradesOrderedByExecutedAtDescending()
    {
        var userId = Guid.NewGuid();
        await SeedAsync(
            CreateTrade(userId, executedAt: 1000),
            CreateTrade(userId, executedAt: 3000),
            CreateTrade(userId, executedAt: 2000)
        );

        var result = await _sut.GetUserTrades(userId);

        result.Should().BeInDescendingOrder(t => t.ExecutedAt);
    }

    [Fact]
    public async Task GetUserTrades_WhenNoTradesExist_ThrowsNotFoundException()
    {
        var act = async () => await _sut.GetUserTrades(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- Helpers ---

    private static Trade CreateTrade(Guid userId, long executedAt = 1000) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        AmountInvested = 1000m,
        Type = "long",
        Symbol = "AAPL",
        StartDate = 1000,
        ExecutedAt = executedAt
    };

    private async Task SeedAsync(params Trade[] trades)
    {
        _db.Trades.AddRange(trades);
        await _db.SaveChangesAsync();
    }
}
