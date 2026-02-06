using HotelSearch.Application.Interfaces;
using HotelSearch.Domain.Common;
using HotelSearch.Domain.Entities;

namespace HotelSearch.Infrastructure.Services;

/// <summary>
/// Hotel search service implementing bounding box pre-filtering with Haversine distance calculation.
///
/// Performance Optimization Strategy:
/// 1. BOUNDING BOX PRE-FILTER: O(n) with simple comparisons (very fast)
///    - Eliminates hotels that are definitely outside the search radius
///    - Uses approximate lat/lon deltas based on Earth's geometry
///    - May include some extra hotels near corners (false positives) but NEVER excludes valid ones
///
/// 2. HAVERSINE DISTANCE: O(m) where m = hotels in bounding box
///    - Expensive trigonometric calculation (sin, cos, atan2, sqrt)
///    - Only performed for candidates that passed bounding box filter
///    - Provides exact great-circle distance on Earth's surface
///
/// Time Complexity: O(n) total, but with significant constant factor reduction
/// Space Complexity: O(m) where m = hotels within bounding box
/// </summary>
public sealed class HotelSearchService : IHotelSearchService
{
    /// <inheritdoc />
    public IReadOnlyList<HotelSearchResultItem> Search(
        IEnumerable<Hotel> hotels,
        double userLatitude,
        double userLongitude,
        double? radiusKm = null)
    {
        ArgumentNullException.ThrowIfNull(hotels);

        // Validate coordinates upfront - fail fast with clear error message
        ValidateCoordinates(userLatitude, userLongitude);

        var effectiveRadius = radiusKm ?? GeoConstants.DefaultSearchRadiusKm;

        // STEP 1: Calculate bounding box for fast pre-filtering
        // This eliminates most hotels without expensive Haversine calculations
        var boundingBox = CalculateBoundingBox(userLatitude, userLongitude, effectiveRadius);

        // STEP 2: Filter hotels within bounding box (fast)
        // Then calculate exact distance only for candidates (expensive)
        var candidatesWithDistance = hotels
            .Where(hotel => IsInBoundingBox(hotel, boundingBox))
            .Select(hotel => new HotelSearchResultItem(
                hotel,
                CalculateDistanceKm(userLatitude, userLongitude, hotel.Location.Latitude, hotel.Location.Longitude)))
            .Where(item => item.DistanceKm <= effectiveRadius) // Final filter with exact distance
            .ToList();

        // STEP 3: Sort by combined price and distance score
        if (candidatesWithDistance.Count <= 1)
        {
            return candidatesWithDistance;
        }

        return SortByPriceAndDistance(candidatesWithDistance);
    }

    /// <inheritdoc />
    public double CalculateDistanceKm(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude)
    {
        // Haversine Formula:
        // a = sin²(Δlat/2) + cos(originLat) * cos(destLat) * sin²(Δlon/2)
        // c = 2 * atan2(√a, √(1-a))
        // distance = R * c
        //
        // Where:
        // - Δlat = destinationLatitude - originLatitude (in radians)
        // - Δlon = destinationLongitude - originLongitude (in radians)
        // - R = Earth's radius

        var originLatRad = GeoConstants.DegreesToRadians(originLatitude);
        var destinationLatRad = GeoConstants.DegreesToRadians(destinationLatitude);
        var deltaLat = GeoConstants.DegreesToRadians(destinationLatitude - originLatitude);
        var deltaLon = GeoConstants.DegreesToRadians(destinationLongitude - originLongitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(originLatRad) * Math.Cos(destinationLatRad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return GeoConstants.EarthRadiusKm * c;
    }

    /// <summary>
    /// Calculates a geographic bounding box around a center point.
    ///
    /// The bounding box is an approximation that:
    /// - MAY include hotels slightly outside the true radius (near corners)
    /// - NEVER excludes hotels that are within the true radius
    ///
    /// This is acceptable because we apply exact Haversine filtering afterward.
    ///
    /// Formulas:
    /// - deltaLat = radius / 111 km (constant)
    /// - deltaLon = radius / (111 * cos(latitude)) (varies with latitude)
    ///
    /// Edge cases handled:
    /// - Near poles: deltaLon increases dramatically (cos approaches 0)
    /// - At equator: deltaLon = deltaLat (cos(0) = 1)
    /// </summary>
    private static BoundingBox CalculateBoundingBox(double centerLat, double centerLon, double radiusKm)
    {
        // Latitude delta is constant regardless of position
        var deltaLat = radiusKm / GeoConstants.KmPerDegreeLat;

        // Longitude delta varies with latitude
        // At the equator: 1° longitude ≈ 111 km
        // At 60°N: 1° longitude ≈ 55 km
        // Near poles: 1° longitude approaches 0 km
        var cosLat = Math.Cos(GeoConstants.DegreesToRadians(centerLat));

        // Prevent division by zero near poles
        // At exactly ±90°, longitude becomes meaningless anyway
        var deltaLon = cosLat > GeoConstants.PolarCosineThreshold
            ? radiusKm / (GeoConstants.KmPerDegreeLat * cosLat)
            : GeoConstants.PolarLongitudeRange; // Near poles, include all longitudes

        return new BoundingBox(
            MinLat: centerLat - deltaLat,
            MaxLat: centerLat + deltaLat,
            MinLon: centerLon - deltaLon,
            MaxLon: centerLon + deltaLon);
    }

    /// <summary>
    /// Checks if a hotel is within the bounding box.
    ///
    /// This is a fast O(1) check using simple comparisons.
    /// Hotels outside the bounding box are immediately rejected
    /// without expensive Haversine calculations.
    ///
    /// Note: Handles antimeridian crossing (where MinLon > MaxLon).
    /// </summary>
    private static bool IsInBoundingBox(Hotel hotel, BoundingBox box)
    {
        var lat = hotel.Location.Latitude;
        var lon = hotel.Location.Longitude;

        // Check latitude (always simple)
        if (lat < box.MinLat || lat > box.MaxLat)
        {
            return false;
        }

        // Check longitude (handle antimeridian crossing)
        if (box.MinLon <= box.MaxLon)
        {
            // Normal case: bounding box doesn't cross antimeridian
            return lon >= box.MinLon && lon <= box.MaxLon;
        }
        else
        {
            // Antimeridian crossing case: box wraps around ±180°
            // Hotel is inside if it's in the "western" part OR "eastern" part
            return lon >= box.MinLon || lon <= box.MaxLon;
        }
    }

    /// <summary>
    /// Sorts hotels by a combined normalized score of price and distance.
    ///
    /// Algorithm:
    /// 1. Find min/max for both price and distance
    /// 2. Normalize each to 0-1 range
    /// 3. Calculate weighted score: (normalizedPrice * 0.5) + (normalizedDistance * 0.5)
    /// 4. Sort by score ascending (lower = better)
    ///
    /// When all values are equal (range = 0), normalized value is 0.
    /// </summary>
    private static List<HotelSearchResultItem> SortByPriceAndDistance(List<HotelSearchResultItem> items)
    {
        var minPrice = items.Min(h => h.Hotel.PricePerNight);
        var maxPrice = items.Max(h => h.Hotel.PricePerNight);
        var priceRange = maxPrice - minPrice;

        var minDistance = items.Min(h => h.DistanceKm);
        var maxDistance = items.Max(h => h.DistanceKm);
        var distanceRange = maxDistance - minDistance;

        return items
            .Select(item =>
            {
                // Normalize price to 0-1 range
                var normalizedPrice = priceRange > 0
                    ? (double)((item.Hotel.PricePerNight - minPrice) / priceRange)
                    : 0;

                // Normalize distance to 0-1 range
                var normalizedDistance = distanceRange > 0
                    ? (item.DistanceKm - minDistance) / distanceRange
                    : 0;

                // Calculate combined score
                var score = (normalizedPrice * GeoConstants.PriceWeight) + (normalizedDistance * GeoConstants.DistanceWeight);

                return (Item: item, Score: score);
            })
            .OrderBy(x => x.Score)
            .Select(x => x.Item)
            .ToList();
    }

    /// <summary>
    /// Validates that coordinates are within valid geographic ranges.
    /// Fails fast with clear error messages.
    /// </summary>
    private static void ValidateCoordinates(double latitude, double longitude)
    {
        if (!GeoConstants.IsValidLatitude(latitude))
        {
            throw new ArgumentOutOfRangeException(
                nameof(latitude),
                latitude,
                GeoConstants.LatitudeErrorMessage);
        }

        if (!GeoConstants.IsValidLongitude(longitude))
        {
            throw new ArgumentOutOfRangeException(
                nameof(longitude),
                longitude,
                GeoConstants.LongitudeErrorMessage);
        }
    }

    /// <summary>
    /// Represents a geographic bounding box for fast pre-filtering.
    /// </summary>
    private readonly record struct BoundingBox(
        double MinLat,
        double MaxLat,
        double MinLon,
        double MaxLon);
}
