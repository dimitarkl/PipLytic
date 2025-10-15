// ...existing code...

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using server.Exceptions;

namespace server.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception at {Path} {Method}. User: {User}",
                context.Request.Path,
                context.Request.Method);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var errors = new List<string>();

        switch (exception)
        {
            case NotFoundException e:
                statusCode = HttpStatusCode.NotFound;
                errors.Add(e.Message);
                break;
            case ForbiddenException e:
                statusCode = HttpStatusCode.Forbidden;
                errors.Add(e.Message);
                break;
            case InvalidCredentialsException e:
                statusCode = HttpStatusCode.BadRequest;
                errors.Add(e.Message);
                break;
            case QuotaExceededException e:
                statusCode = (HttpStatusCode)429;
                errors.Add(e.Message);
                break;
            case DataExpiredException e:
                statusCode = HttpStatusCode.BadRequest;
                errors.Add(e.Message);
                break;
            case UnauthorizedAccessException e:
                statusCode = HttpStatusCode.Unauthorized;
                errors.Add(e.Message ?? "Unauthorized");
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                errors.Add("An unexpected error occurred. Please try again later.");
                break;
        }

        var response = new
        {
            message = errors.FirstOrDefault(),
            details = errors.Count > 1 ? errors : null,
        };

        var payload = JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(payload);
    }
}