namespace server.Extensions;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public static class HttpContextExtensions
{
    public static Guid GetUserId(this HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User is not authenticated.");

        return Guid.Parse(userIdClaim);
    }
}