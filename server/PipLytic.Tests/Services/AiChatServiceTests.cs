using System.Net;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using PipLytic.Api.Data;
using PipLytic.Api.Entities;
using PipLytic.Api.Enums;
using PipLytic.Api.Exceptions;
using PipLytic.Api.Models;
using PipLytic.Api.Models.GeminiApi;
using PipLytic.Api.Services;

namespace PipLytic.Tests.Services;

public class AiChatServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _memoryCache;
    private readonly GeminiChatCache _geminiCache;
    private readonly IMarketDataService _marketDataService;
    private readonly IConfiguration _config;

    public AiChatServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _geminiCache = new GeminiChatCache(_memoryCache, TimeSpan.FromMinutes(5));
        _marketDataService = A.Fake<IMarketDataService>();

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:GeminiApiKey"] = "test-key",
                ["AppSettings:GeminiSystemInstructions"] = "You are a trading assistant."
            })
            .Build();
    }

    public void Dispose()
    {
        _db.Dispose();
        _memoryCache.Dispose();
    }

    // --- SendMessage ---

    [Fact]
    public async Task SendMessage_WhenUserNotFound_ThrowsNotFoundException()
    {
        var sut = CreateService();

        var act = async () => await sut.SendMessage(
            Guid.NewGuid(),
            new ChatRequest { Message = "hello", Symbol = "AAPL", EndDate = 1000 });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("User Not Found");
    }

    [Fact]
    public async Task SendMessage_WhenMarketDataExpired_ThrowsDataExpiredException()
    {
        var user = await SeedUserAsync();
        A.CallTo(() => _marketDataService.GetUserCacheIfExists(user.Id.ToString(), "AAPL")).Returns(null);
        var sut = CreateService();

        var act = async () => await sut.SendMessage(
            user.Id,
            new ChatRequest { Message = "hello", Symbol = "AAPL", EndDate = 1000 });

        await act.Should().ThrowAsync<DataExpiredException>();
    }

    [Fact]
    public async Task SendMessage_WhenFreeUserExceedsQuota_ThrowsQuotaExceededException()
    {
        var user = await SeedUserAsync(userType: EUserType.Free, aiUsesLeft: 0);
        A.CallTo(() => _marketDataService.GetUserCacheIfExists(user.Id.ToString(), "AAPL"))
            .Returns(BuildFakeTimeSeries());
        var sut = CreateService();

        var act = async () => await sut.SendMessage(
            user.Id,
            new ChatRequest { Message = "hello", Symbol = "AAPL", EndDate = long.MaxValue });

        await act.Should().ThrowAsync<QuotaExceededException>();
    }

    [Fact]
    public async Task SendMessage_WhenFreeUserHasUsesLeft_ReturnsResponse()
    {
        var user = await SeedUserAsync(userType: EUserType.Free);
        A.CallTo(() => _marketDataService.GetUserCacheIfExists(user.Id.ToString(), "AAPL"))
            .Returns(BuildFakeTimeSeries());
        var sut = CreateService();

        var result = await sut.SendMessage(
            user.Id,
            new ChatRequest { Message = "Analyse my trade", Symbol = "AAPL", EndDate = long.MaxValue });

        result.Should().NotBeNull();
        result.Message.Should().Be("Test AI response");
        result.Role.Should().Be("model");
    }

    [Fact]
    public async Task SendMessage_WhenFreeUserSendsMessage_DecrementsAiUsesLeft()
    {
        var user = await SeedUserAsync(userType: EUserType.Free);
        A.CallTo(() => _marketDataService.GetUserCacheIfExists(user.Id.ToString(), "AAPL"))
            .Returns(BuildFakeTimeSeries());
        var sut = CreateService();

        await sut.SendMessage(user.Id, new ChatRequest { Message = "hello", Symbol = "AAPL", EndDate = long.MaxValue });

        var updated = await _db.Users.FindAsync(user.Id);
        updated!.AiUsesLeft.Should().BeLessThan(5);
    }

    [Fact]
    public async Task SendMessage_WhenPremiumUser_DoesNotConsumeQuota()
    {
        var user = await SeedUserAsync(userType: EUserType.Premium);
        A.CallTo(() => _marketDataService.GetUserCacheIfExists(user.Id.ToString(), "AAPL"))
            .Returns(BuildFakeTimeSeries());
        var sut = CreateService();

        var result = await sut.SendMessage(
            user.Id,
            new ChatRequest { Message = "hello", Symbol = "AAPL", EndDate = long.MaxValue });

        result.Should().NotBeNull();
        var updated = await _db.Users.FindAsync(user.Id);
        // Premium users skip quota, so AiUsesLeft unchanged from initial value
        updated!.AiUsesLeft.Should().Be(5);
    }

    // --- GetMessageHistory ---

    [Fact]
    public void GetMessageHistory_WhenNoHistoryExists_ReturnsEmptyList()
    {
        var sut = CreateService();

        var result = sut.GetMessageHistory(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMessageHistory_AfterSendingMessages_ReturnsMappedHistory()
    {
        var user = await SeedUserAsync(userType: EUserType.Premium);
        A.CallTo(() => _marketDataService.GetUserCacheIfExists(user.Id.ToString(), "AAPL"))
            .Returns(BuildFakeTimeSeries());
        var sut = CreateService();

        await sut.SendMessage(user.Id, new ChatRequest { Message = "hello", Symbol = "AAPL", EndDate = long.MaxValue });

        var history = sut.GetMessageHistory(user.Id);

        history.Should().NotBeEmpty();
        history.Should().Contain(h => h.Role == "user" || h.Role == "model");
    }

    // --- Helpers ---

    private AiChatService CreateService()
    {
        var responseJson = BuildGeminiResponse("Test AI response");
        var handler = new FakeHttpMessageHandler(responseJson);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
        };
        var factory = A.Fake<IHttpClientFactory>();
        A.CallTo(() => factory.CreateClient("GeminiApi")).Returns(httpClient);
        return new AiChatService(_db, factory, _geminiCache, _config, _marketDataService);
    }

    private async Task<User> SeedUserAsync(EUserType userType = EUserType.Free, int? aiUsesLeft = null)
    {
        var user = new User { Id = Guid.NewGuid(), Email = $"{Guid.NewGuid()}@test.com", PasswordHash = "hash", UserType = userType };

        if (aiUsesLeft.HasValue)
        {
            // Use reflection to set private fields for quota testing
            typeof(User).GetProperty(nameof(User.AiUsesLeft))!.SetValue(user, aiUsesLeft.Value);
            typeof(User).GetProperty(nameof(User.AiResetAt))!.SetValue(user, DateTime.UtcNow.AddHours(6));
        }

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    private static TimeSeriesResponse BuildFakeTimeSeries() => new()
    {
        Meta = new TimeSeriesResponseMeta { Symbol = "AAPL", Interval = "5min", Currency = "USD", ExchangeTimezone = "America/New_York" },
        Status = "ok",
        Values =
        [
            new TimeSeriesResponseValue { DateTimeRaw = "2024-01-15 09:30:00", Open = 180m, High = 185m, Low = 179m, Close = 182m, Volume = 1000 }
        ]
    };

    private static string BuildGeminiResponse(string text) => $$"""
        {
            "candidates": [
                {
                    "content": {
                        "parts": [ { "text": "{{text}}" } ]
                    }
                }
            ]
        }
        """;

    private sealed class FakeHttpMessageHandler(string response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response)
            });
    }
}
