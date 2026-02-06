namespace HotelSearch.Domain.Common;

/// <summary>
/// Centralized geographic and calculation constants used throughout the application.
/// All coordinate validation, distance calculations, and geo-related operations should use these constants.
/// </summary>
public static class GeoConstants
{
    #region Coordinate Bounds

    /// <summary>
    /// Minimum valid latitude (-90° = South Pole).
    /// </summary>
    public const double MinLatitude = -90.0;

    /// <summary>
    /// Maximum valid latitude (90° = North Pole).
    /// </summary>
    public const double MaxLatitude = 90.0;

    /// <summary>
    /// Minimum valid longitude (-180° = antimeridian, west).
    /// </summary>
    public const double MinLongitude = -180.0;

    /// <summary>
    /// Maximum valid longitude (180° = antimeridian, east).
    /// </summary>
    public const double MaxLongitude = 180.0;

    #endregion

    #region Earth Measurements

    /// <summary>
    /// Mean radius of Earth in kilometers.
    /// Used in Haversine formula for great-circle distance calculation.
    /// </summary>
    public const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// Approximate kilometers per degree of latitude.
    /// This is constant regardless of position on Earth.
    /// Derived from: Earth's circumference (40,075 km) / 360 degrees ≈ 111.32 km
    /// </summary>
    public const double KmPerDegreeLat = 111.0;

    /// <summary>
    /// Earth's approximate circumference in kilometers.
    /// </summary>
    public const double EarthCircumferenceKm = 40_075.0;

    #endregion

    #region Search Defaults

    /// <summary>
    /// Default search radius when none specified (in km).
    /// Set to half Earth's circumference for effectively unlimited global search.
    /// </summary>
    public const double DefaultSearchRadiusKm = 20_000.0;

    /// <summary>
    /// Threshold for cosine of latitude below which we consider the point to be "near a pole".
    /// Used to handle edge cases in longitude calculations near poles.
    /// </summary>
    public const double PolarCosineThreshold = 0.0001;

    /// <summary>
    /// When near poles, include all longitudes (180 degrees in each direction).
    /// </summary>
    public const double PolarLongitudeRange = 180.0;

    #endregion

    #region Sorting Weights

    /// <summary>
    /// Weight applied to normalized price in combined sorting score.
    /// Lower price = better ranking.
    /// </summary>
    public const double PriceWeight = 0.5;

    /// <summary>
    /// Weight applied to normalized distance in combined sorting score.
    /// Closer distance = better ranking.
    /// </summary>
    public const double DistanceWeight = 0.5;

    #endregion

    #region Conversion Factors

    /// <summary>
    /// Conversion factor from degrees to radians.
    /// radians = degrees * DegreesToRadiansFactor
    /// </summary>
    public const double DegreesToRadiansFactor = Math.PI / 180.0;

    #endregion

    #region Validation Messages

    /// <summary>
    /// Standard error message for invalid latitude values.
    /// </summary>
    public const string LatitudeErrorMessage = "Latitude must be between -90 and 90 degrees.";

    /// <summary>
    /// Standard error message for invalid longitude values.
    /// </summary>
    public const string LongitudeErrorMessage = "Longitude must be between -180 and 180 degrees.";

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates that a latitude value is within the valid range.
    /// </summary>
    public static bool IsValidLatitude(double latitude)
        => latitude >= MinLatitude && latitude <= MaxLatitude;

    /// <summary>
    /// Validates that a longitude value is within the valid range.
    /// </summary>
    public static bool IsValidLongitude(double longitude)
        => longitude >= MinLongitude && longitude <= MaxLongitude;

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    public static double DegreesToRadians(double degrees)
        => degrees * DegreesToRadiansFactor;

    #endregion
}
