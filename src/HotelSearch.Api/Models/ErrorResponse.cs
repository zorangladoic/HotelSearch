namespace HotelSearch.Api.Models;

/// <summary>
/// Standard error response format for API errors.
/// </summary>
/// <param name="Message">High-level error message describing the failure.</param>
/// <param name="Errors">Detailed error messages (e.g., validation errors).</param>
/// <param name="CorrelationId">Request correlation ID for tracing.</param>
public record ErrorResponse(string Message, string[] Errors, string CorrelationId);
