using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Middleware;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Not Found"),
            InvalidOperationException => ((int)HttpStatusCode.Conflict, "Business Rule Violation"),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Bad Request"),
            _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "An unhandled exception occurred");
        }
        else
        {
            logger.LogWarning("Business exception: {Message}", exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
