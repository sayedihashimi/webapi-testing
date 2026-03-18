using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

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
                Title = "Not Found",
                Detail = exception.Message,
                Type = "https://tools.ietf.org/html/rfc7807"
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Detail = exception.Message,
                Type = "https://tools.ietf.org/html/rfc7807"
            }
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? 500;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
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
