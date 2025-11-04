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
                "Unhandled exception at {Path} {Method}",
                context.Request.Path,
                context.Request.Method);

            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var errors = new List<string>();

        switch (exception)
        {
            case NotImplementedException e:
                statusCode = HttpStatusCode.NotImplemented;
                errors.Add("This feature is not yet implemented.");
                _logger.LogWarning("NotImplementedException: {Message} at {Path}", e.Message, context.Request.Path);
                break;
            case NotFoundException e:
                statusCode = HttpStatusCode.NotFound;
                errors.Add(e.Message);
                _logger.LogWarning("NotFoundException: {Message} at {Path}", e.Message, context.Request.Path);
                break;
            case ForbiddenException e:
                statusCode = HttpStatusCode.Forbidden;
                errors.Add(e.Message);
                _logger.LogWarning("ForbiddenException: {Message} at {Path}", e.Message, context.Request.Path);
                break;
            case InvalidCredentialsException e:
                statusCode = HttpStatusCode.Unauthorized;
                errors.Add(e.Message);
                _logger.LogWarning("InvalidCredentialsException: {Message} at {Path}", e.Message, context.Request.Path);
                break;
            case QuotaExceededException e:
                statusCode = (HttpStatusCode)429;
                errors.Add(e.Message);
                _logger.LogWarning("QuotaExceededException: {Message} at {Path}", e.Message, context.Request.Path);
                break;
            case DataExpiredException e:
                statusCode = HttpStatusCode.BadRequest;
                errors.Add(e.Message);
                _logger.LogWarning("DataExpiredException: {Message} at {Path}", e.Message, context.Request.Path);
                break;
            case UnauthorizedAccessException e:
                statusCode = HttpStatusCode.Unauthorized;
                errors.Add(e.Message ?? "Unauthorized");
                _logger.LogWarning("UnauthorizedAccessException: {Message} at {Path}", e.Message ?? "Unauthorized", context.Request.Path);
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                errors.Add("An unexpected error occurred. Please try again later.");
                _logger.LogError(exception, "Unexpected exception at {Path}: {Message}", context.Request.Path, exception.Message);
                break;
        }

        var response = new
        {
            message = errors.FirstOrDefault(),
            //details = errors.Count > 1 ? errors : null,
        };

        var payload = JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(payload);
    }
}