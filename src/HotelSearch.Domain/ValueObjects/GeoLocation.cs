using HotelSearch.Domain.Common;

namespace HotelSearch.Domain.ValueObjects;

public sealed class GeoLocation : IEquatable<GeoLocation>
{
    public double Latitude { get; }
    public double Longitude { get; }

    private const double EqualityTolerance = 1e-7;

    private GeoLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoLocation Create(double latitude, double longitude)
    {
        if (double.IsNaN(latitude) || double.IsInfinity(latitude))
            throw new ArgumentOutOfRangeException(nameof(latitude), latitude, "Latitude must be a finite number.");

        if (double.IsNaN(longitude) || double.IsInfinity(longitude))
            throw new ArgumentOutOfRangeException(nameof(longitude), longitude, "Longitude must be a finite number.");

        if (!GeoConstants.IsValidLatitude(latitude))
            throw new ArgumentOutOfRangeException(nameof(latitude), latitude, GeoConstants.LatitudeErrorMessage);

        if (!GeoConstants.IsValidLongitude(longitude))
            throw new ArgumentOutOfRangeException(nameof(longitude), longitude, GeoConstants.LongitudeErrorMessage);

        return new GeoLocation(latitude, longitude);
    }

    public double DistanceTo(GeoLocation destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        // Fast path
        if (ReferenceEquals(this, destination))
            return 0.0;

        var originLatRad = GeoConstants.DegreesToRadians(Latitude);
        var destinationLatRad = GeoConstants.DegreesToRadians(destination.Latitude);
        var deltaLat = GeoConstants.DegreesToRadians(destination.Latitude - Latitude);
        var deltaLon = GeoConstants.DegreesToRadians(destination.Longitude - Longitude);

        var sinDeltaLatHalf = Math.Sin(deltaLat / 2);
        var sinDeltaLonHalf = Math.Sin(deltaLon / 2);

        var a = sinDeltaLatHalf * sinDeltaLatHalf +
                Math.Cos(originLatRad) * Math.Cos(destinationLatRad) *
                sinDeltaLonHalf * sinDeltaLonHalf;

        // Clamp to [0,1] to mitigate floating-point rounding errors that could produce NaN
        a = Math.Clamp(a, 0.0, 1.0);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return GeoConstants.EarthRadiusKm * c;
    }

    public bool Equals(GeoLocation? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;

        return Math.Abs(Latitude - other.Latitude) <= EqualityTolerance &&
               Math.Abs(Longitude - other.Longitude) <= EqualityTolerance;
    }

    public override bool Equals(object? obj) => Equals(obj as GeoLocation);

    public override int GetHashCode()
    {
        // Quantize coordinates to the equality tolerance to preserve equals/hash contract.
        // Convert to long after scaling by 1/tolerance to avoid double precision issues.
        var scale = 1.0 / EqualityTolerance;
        var latQuant = (long)Math.Round(Latitude * scale);
        var lonQuant = (long)Math.Round(Longitude * scale);
        return HashCode.Combine(latQuant, lonQuant);
    }

    public override string ToString() => $"({Latitude:F7}, {Longitude:F7})";

    public static bool operator ==(GeoLocation? left, GeoLocation? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(GeoLocation? left, GeoLocation? right) => !(left == right);
}
