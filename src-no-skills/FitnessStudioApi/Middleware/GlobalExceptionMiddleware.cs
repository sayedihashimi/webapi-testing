using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var statusCode = exception switch
        {
            BusinessRuleException => StatusCodes.Status400BadRequest,
            ConflictException => StatusCodes.Status409Conflict,
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = exception switch
            {
                BusinessRuleException => "Business Rule Violation",
                ConflictException => "Conflict",
                NotFoundException => "Not Found",
                _ => "Internal Server Error"
            },
            Detail = exception is BusinessRuleException or ConflictException or NotFoundException
                ? exception.Message
                : "An unexpected error occurred. Please try again later.",
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
