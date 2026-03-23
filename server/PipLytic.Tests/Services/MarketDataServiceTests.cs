using System.Net;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using PipLytic.Api.Models;
using PipLytic.Api.Services;

namespace PipLytic.Tests.Services;

public class MarketDataServiceTests : IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;

    public MarketDataServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AppSettings:TwelveDataApiKey"] = "test-key" })
            .Build();
    }

    public void Dispose() => _cache.Dispose();

    // --- GetUserCacheIfExists ---

    [Fact]
    public void GetUserCacheIfExists_WhenNoEntryExists_ReturnsNull()
    {
        var sut = CreateService();

        var result = sut.GetUserCacheIfExists("user-no-entry", "AAPL");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserCacheIfExists_AfterQueryingData_ReturnsCachedData()
    {
        var userId = Guid.NewGuid().ToString();
        var symbol = $"SYM_{Guid.NewGuid():N}";
        int callCount = 0;
        var sut = CreateService(symbol: symbol, onApiCall: () => callCount++);

        await sut.QueryStocksData(new MarketDataDto { Symbol = symbol, Interval = "5min" }, userId);

        var cached = sut.GetUserCacheIfExists(userId, symbol);
        cached.Should().NotBeNull();
    }

    // --- QueryStocksData ---

    [Fact]
    public async Task QueryStocksData_WhenNoCache_FetchesFromApiAndReturnsResult()
    {
        var symbol = $"SYM_{Guid.NewGuid():N}";
        int callCount = 0;
        var sut = CreateService(symbol: symbol, onApiCall: () => callCount++);

        var result = await sut.QueryStocksData(new MarketDataDto { Symbol = symbol, Interval = "5min" }, "user1");

        callCount.Should().Be(1);
        result.Should().NotBeNull();
        result.Status.Should().Be("ok");
    }

    [Fact]
    public async Task QueryStocksData_WhenUserCacheExists_DoesNotCallApi()
    {
        var userId = Guid.NewGuid().ToString();
        var symbol = $"SYM_{Guid.NewGuid():N}";
        int callCount = 0;
        var sut = CreateService(symbol: symbol, onApiCall: () => callCount++);

        // First call — populates both shared and user cache
        await sut.QueryStocksData(new MarketDataDto { Symbol = symbol, Interval = "5min" }, userId);
        callCount = 0;

        // Second call with same user — should hit user cache
        var result = await sut.QueryStocksData(new MarketDataDto { Symbol = symbol, Interval = "5min" }, userId);

        callCount.Should().Be(0);
        result.Should().NotBeNull();
    }

    // --- ContinueStocksData ---

    [Fact]
    public async Task ContinueStocksData_WithNullLastDate_ThrowsArgumentException()
    {
        var sut = CreateService();

        var act = async () => await sut.ContinueStocksData(
            new MarketDataDto { Symbol = "AAPL", Interval = "5min", LastDate = null }, "user1");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ContinueStocksData_WithValidLastDate_ReturnsNextMonthData()
    {
        var symbol = $"SYM_{Guid.NewGuid():N}";
        var sut = CreateService(symbol: symbol);
        var lastDate = new DateTimeOffset(2024, 1, 31, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();

        var result = await sut.ContinueStocksData(
            new MarketDataDto { Symbol = symbol, Interval = "5min", LastDate = lastDate }, "user1");

        result.Should().NotBeNull();
        result.Status.Should().Be("ok");
    }

    // --- RefreshStocksData ---

    [Fact]
    public async Task RefreshStocksData_ReturnsNewData()
    {
        var symbol = $"SYM_{Guid.NewGuid():N}";
        var sut = CreateService(symbol: symbol);

        var result = await sut.RefreshStocksData(
            new MarketDataDto { Symbol = symbol, Interval = "5min" }, "user1");

        result.Should().NotBeNull();
        result.Status.Should().Be("ok");
    }

    [Fact]
    public async Task RefreshStocksData_ClearsOldUserCache()
    {
        var userId = Guid.NewGuid().ToString();
        var symbol = $"SYM_{Guid.NewGuid():N}";
        int callCount = 0;
        var sut = CreateService(symbol: symbol, onApiCall: () => callCount++);

        // Populate user cache
        await sut.QueryStocksData(new MarketDataDto { Symbol = symbol, Interval = "5min" }, userId);
        callCount = 0;

        // Refresh should fetch fresh data (not from user cache)
        await sut.RefreshStocksData(new MarketDataDto { Symbol = symbol, Interval = "5min" }, userId);

        callCount.Should().Be(1);
    }

    // --- Helpers ---

    private MarketDataService CreateService(string? symbol = null, Action? onApiCall = null)
    {
        var fakeJson = BuildValidTimeSeriesJson(symbol ?? "TEST");
        var handler = new CountingFakeHandler(fakeJson, HttpStatusCode.OK, onApiCall);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.twelvedata.com/") };
        var factory = A.Fake<IHttpClientFactory>();
        A.CallTo(() => factory.CreateClient("TwelveData")).Returns(httpClient);
        return new MarketDataService(factory, _config, _cache);
    }

    private static string BuildValidTimeSeriesJson(string symbol) => $$"""
        {
            "meta": { "symbol": "{{symbol}}", "interval": "5min", "currency": "USD", "exchange_timezone": "America/New_York" },
            "values": [
                { "datetime": "2024-01-15 09:30:00", "open": "182.50", "high": "183.00", "low": "181.50", "close": "182.75", "volume": "1000" },
                { "datetime": "2024-01-15 09:35:00", "open": "182.75", "high": "183.50", "low": "182.00", "close": "183.25", "volume": "1200" },
                { "datetime": "2024-01-15 09:40:00", "open": "183.25", "high": "184.00", "low": "183.00", "close": "183.75", "volume": "900" }
            ],
            "status": "ok"
        }
        """;

    private sealed class CountingFakeHandler(string response, HttpStatusCode statusCode, Action? onCall) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            onCall?.Invoke();
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(response)
            });
        }
    }
}
