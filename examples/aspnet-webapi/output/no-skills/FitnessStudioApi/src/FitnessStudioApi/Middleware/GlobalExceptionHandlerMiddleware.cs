using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = ex.StatusCode,
                Title = ex.Title,
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7807"
            };
            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = 500,
                Title = "An unexpected error occurred",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7807"
            };
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}

public class BusinessRuleException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }

    public BusinessRuleException(string message, int statusCode = 400, string title = "Business Rule Violation")
        : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }
}
