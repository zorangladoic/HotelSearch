namespace HotelSearch.Domain.ValueObjects;

public sealed class GeoLocation : IEquatable<GeoLocation>
{
    private const double EarthRadiusKm = 6371.0;

    public double Latitude { get; }
    public double Longitude { get; }

    private GeoLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoLocation Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), latitude, "Latitude must be between -90 and 90 degrees.");

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), longitude, "Longitude must be between -180 and 180 degrees.");

        return new GeoLocation(latitude, longitude);
    }

    public double DistanceTo(GeoLocation other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var lat1Rad = DegreesToRadians(Latitude);
        var lat2Rad = DegreesToRadians(other.Latitude);
        var deltaLat = DegreesToRadians(other.Latitude - Latitude);
        var deltaLon = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

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
