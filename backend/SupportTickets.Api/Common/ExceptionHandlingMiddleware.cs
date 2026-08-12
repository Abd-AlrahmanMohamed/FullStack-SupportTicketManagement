using System.Net;
using System.Text.Json;
using SupportTickets.Application.Common.Exceptions;

namespace SupportTickets.Api.Common;

public class ErrorResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// Single place that turns exceptions into consistent JSON responses, so controllers
/// and handlers never need their own try/catch blocks.
/// </summary>
public class ExceptionHandlingMiddleware
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            ValidationException validationException => new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest,
                Message = "One or more validation failures have occurred.",
                Errors = validationException.Errors
            },
            NotFoundException => new ErrorResponse
            {
                Status = (int)HttpStatusCode.NotFound,
                Message = exception.Message
            },
            ForbiddenException => new ErrorResponse
            {
                Status = (int)HttpStatusCode.Forbidden,
                Message = exception.Message
            },
            BusinessRuleException => new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest,
                Message = exception.Message
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Message = exception.Message
            },
            _ => new ErrorResponse
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Message = "An unexpected error occurred. Please try again later."
            }
        };

        if (response.Status == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("{ExceptionType} handled for {Method} {Path}: {Message}",
                exception.GetType().Name, context.Request.Method, context.Request.Path, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.Status;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
