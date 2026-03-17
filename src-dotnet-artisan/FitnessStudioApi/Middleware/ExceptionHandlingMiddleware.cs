using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = exception switch
        {
            BusinessRuleException bre => new ProblemDetails
            {
                Status = bre.StatusCode,
                Title = bre.Title,
                Detail = bre.Message,
                Type = "https://tools.ietf.org/html/rfc7807"
            },
            KeyNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                Detail = exception.Message,
                Type = "https://tools.ietf.org/html/rfc7807"
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                Type = "https://tools.ietf.org/html/rfc7807"
            }
        };

        context.Response.StatusCode = problemDetails.Status ?? 500;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}

public class BusinessRuleException(string message, string title = "Business Rule Violation", int statusCode = StatusCodes.Status400BadRequest) : Exception(message)
{
    public string Title { get; } = title;
    public int StatusCode { get; } = statusCode;
}

public class ConflictException(string message) : BusinessRuleException(message, "Conflict", StatusCodes.Status409Conflict);
