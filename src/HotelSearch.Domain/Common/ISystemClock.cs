namespace HotelSearch.Domain.Common;

/// <summary>
/// Abstraction for system time, enabling testability and time-dependent logic mocking.
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}

/// <summary>
/// Default implementation that returns the actual system time.
/// </summary>
public sealed class SystemClock : ISystemClock
{
    /// <summary>
    /// Singleton instance of the system clock.
    /// </summary>
    public static readonly ISystemClock Instance = new SystemClock();

    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
