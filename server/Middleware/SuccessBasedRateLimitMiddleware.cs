using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace server.Middleware;

public class SuccessBasedRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SuccessBasedRateLimitMiddleware> _logger;

    public SuccessBasedRateLimitMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<SuccessBasedRateLimitMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var rateLimitAttribute = endpoint?.Metadata.GetMetadata<SuccessBasedRateLimitAttribute>();

        if (rateLimitAttribute == null)
        {
            await _next(context);
            return;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var cacheKey = $"ratelimit:{rateLimitAttribute.PolicyName}:{userId}";

        // Check if user has exceeded rate limit
        if (_cache.TryGetValue(cacheKey, out DateTime lastSuccessTime))
        {
            var timeSinceLastSuccess = DateTime.UtcNow - lastSuccessTime;
            
            if (timeSinceLastSuccess < rateLimitAttribute.Window)
            {
                var retryAfter = (rateLimitAttribute.Window - timeSinceLastSuccess).TotalSeconds;
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Too many requests. Please try again later.",
                    retryAfter = $"{Math.Ceiling(retryAfter)} seconds"
                });
                return;
            }
        }

        // Allow the request to proceed
        await _next(context);

        // Only record successful requests (2xx status codes)
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            _cache.Set(cacheKey, DateTime.UtcNow, rateLimitAttribute.Window);
            _logger.LogInformation("Rate limit recorded for user {UserId} on policy {Policy}", userId, rateLimitAttribute.PolicyName);
        }
        else
        {
            _logger.LogInformation("Request failed with status {StatusCode}, not counting against rate limit", context.Response.StatusCode);
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SuccessBasedRateLimitAttribute : Attribute
{
    public string PolicyName { get; }
    public TimeSpan Window { get; }

    public SuccessBasedRateLimitAttribute(string policyName, int windowMinutes = 1)
    {
        PolicyName = policyName;
        Window = TimeSpan.FromMinutes(windowMinutes);
    }
}

