using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.Services;

namespace VetClinicApi.Middleware;

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
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Business Rule Violation",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7807"
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An unexpected error occurred",
                Detail = "An internal server error occurred. Please try again later.",
                Type = "https://tools.ietf.org/html/rfc7807"
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
