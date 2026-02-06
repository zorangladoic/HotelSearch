using HotelSearch.Domain.Common;

namespace HotelSearch.Domain.ValueObjects;

public sealed class GeoLocation : IEquatable<GeoLocation>
{
    public double Latitude { get; }
    public double Longitude { get; }

    private GeoLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoLocation Create(double latitude, double longitude)
    {
        if (!GeoConstants.IsValidLatitude(latitude))
            throw new ArgumentOutOfRangeException(nameof(latitude), latitude, GeoConstants.LatitudeErrorMessage);

        if (!GeoConstants.IsValidLongitude(longitude))
            throw new ArgumentOutOfRangeException(nameof(longitude), longitude, GeoConstants.LongitudeErrorMessage);

        return new GeoLocation(latitude, longitude);
    }

    public double DistanceTo(GeoLocation destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        var originLatRad = GeoConstants.DegreesToRadians(Latitude);
        var destinationLatRad = GeoConstants.DegreesToRadians(destination.Latitude);
        var deltaLat = GeoConstants.DegreesToRadians(destination.Latitude - Latitude);
        var deltaLon = GeoConstants.DegreesToRadians(destination.Longitude - Longitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(originLatRad) * Math.Cos(destinationLatRad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return GeoConstants.EarthRadiusKm * c;
    }

    public bool Equals(GeoLocation? other)
    {
        if (other is null) return false;
        return Math.Abs(Latitude - other.Latitude) < 0.0000001 &&
               Math.Abs(Longitude - other.Longitude) < 0.0000001;
    }

    public override bool Equals(object? obj) => Equals(obj as GeoLocation);
    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);
    public override string ToString() => $"({Latitude:F6}, {Longitude:F6})";

    public static bool operator ==(GeoLocation? left, GeoLocation? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(GeoLocation? left, GeoLocation? right) => !(left == right);
}
