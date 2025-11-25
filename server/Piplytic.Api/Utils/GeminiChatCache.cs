using Microsoft.Extensions.Caching.Memory;
using PipLytic.Api.Models.GeminiApi;

namespace PipLytic.Api.Services;

public class GeminiChatCache
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl;

    public GeminiChatCache(IMemoryCache cache, TimeSpan ttl)
    {
        _cache = cache;
        _ttl = ttl;
    }

    public List<Content> GetHistory(Guid userId)
        => _cache.Get<List<Content>>(userId) ?? new List<Content>();

    public void SetHistory(Guid userId, List<Content> history)
        => _cache.Set(userId, history, _ttl);

    public void ClearHistory(Guid userId)
        => _cache.Remove(userId);
}