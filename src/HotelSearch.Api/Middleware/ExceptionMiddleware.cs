using System.Net;
using System.Text.Json;
using FluentValidation;
using HotelSearch.Domain.Exceptions;

namespace HotelSearch.Api.Middleware;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        var (statusCode, message) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(
                    "Validation failed",
                    validationEx.Errors.Select(e => e.ErrorMessage).ToArray(),
                    correlationId)),

            HotelNotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse(notFoundEx.Message, [], correlationId)),

            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(domainEx.Message, [], correlationId)),

            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(argEx.Message, [], correlationId)),

            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse(
                    "An unexpected error occurred. Please try again later.",
                    [],
                    correlationId))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
        }
        else
        {
            _logger.LogWarning("Handled exception: {Message}. CorrelationId: {CorrelationId}", exception.Message, correlationId);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(message, options));
    }
}

public record ErrorResponse(string Message, string[] Errors, string CorrelationId);
