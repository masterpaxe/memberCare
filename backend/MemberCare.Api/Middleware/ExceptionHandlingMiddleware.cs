using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Middleware;

/// <summary>
/// Global exception handling middleware to catch branch scope violations and other errors.
/// Converts UnauthorizedAccessException to 403 Forbidden responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Branch scope violation: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";

            var response = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Detail = ex.Message ?? "Access denied. You may not have permission to access this resource."
            };

            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var response = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred. Please try again later."
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
