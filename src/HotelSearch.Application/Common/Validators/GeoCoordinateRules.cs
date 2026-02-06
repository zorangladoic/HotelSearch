using FluentValidation;
using HotelSearch.Domain.Common;

namespace HotelSearch.Application.Common.Validators;

/// <summary>
/// FluentValidation extension methods for geographic coordinates.
/// Uses constants from <see cref="GeoConstants"/> for single source of truth.
/// </summary>
public static class GeoCoordinateRules
{
    // Re-export constants for convenience (delegates to GeoConstants)
    public const string LatitudeErrorMessage = GeoConstants.LatitudeErrorMessage;
    public const string LongitudeErrorMessage = GeoConstants.LongitudeErrorMessage;

    /// <summary>
    /// Validates that a latitude value is within the valid range [-90, 90].
    /// </summary>
    public static IRuleBuilderOptions<T, double> ValidLatitude<T>(this IRuleBuilder<T, double> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(GeoConstants.MinLatitude, GeoConstants.MaxLatitude)
            .WithMessage(GeoConstants.LatitudeErrorMessage);
    }

    /// <summary>
    /// Validates that a longitude value is within the valid range [-180, 180].
    /// </summary>
    public static IRuleBuilderOptions<T, double> ValidLongitude<T>(this IRuleBuilder<T, double> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(GeoConstants.MinLongitude, GeoConstants.MaxLongitude)
            .WithMessage(GeoConstants.LongitudeErrorMessage);
    }

    /// <summary>
    /// Validates that a latitude value is within the valid range.
    /// </summary>
    public static bool IsValidLatitude(double latitude) => GeoConstants.IsValidLatitude(latitude);

    /// <summary>
    /// Validates that a longitude value is within the valid range.
    /// </summary>
    public static bool IsValidLongitude(double longitude) => GeoConstants.IsValidLongitude(longitude);
}
