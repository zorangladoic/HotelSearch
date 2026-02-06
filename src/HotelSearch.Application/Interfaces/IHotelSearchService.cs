using HotelSearch.Domain.Entities;

namespace HotelSearch.Application.Interfaces;

/// <summary>
/// Service interface for hotel search operations.
/// Contains business logic for searching, filtering, and sorting hotels.
/// </summary>
public interface IHotelSearchService
{
    /// <summary>
    /// Searches for hotels near a given location using optimized bounding box pre-filtering
    /// followed by exact Haversine distance calculation.
    ///
    /// Algorithm:
    /// 1. Calculate geographic bounding box based on search radius
    /// 2. Filter hotels within bounding box (fast, simple comparisons)
    /// 3. Calculate exact Haversine distance only for candidates
    /// 4. Filter by actual radius and sort by price + distance
    ///
    /// Time Complexity: O(n) where n = total hotels, but with significant constant factor
    /// reduction due to bounding box elimination of Haversine calculations.
    /// </summary>
    /// <param name="hotels">Collection of hotels to search through</param>
    /// <param name="userLatitude">User's latitude in degrees</param>
    /// <param name="userLongitude">User's longitude in degrees</param>
    /// <param name="radiusKm">Optional search radius in kilometers (null = no radius filter)</param>
    /// <returns>Hotels within the search area, sorted by price and distance</returns>
    IReadOnlyList<HotelSearchResultItem> Search(
        IEnumerable<Hotel> hotels,
        double userLatitude,
        double userLongitude,
        double? radiusKm = null);

    /// <summary>
    /// Calculates the distance between two geographic points using the Haversine formula.
    /// </summary>
    /// <param name="originLatitude">Latitude of origin point in degrees</param>
    /// <param name="originLongitude">Longitude of origin point in degrees</param>
    /// <param name="destinationLatitude">Latitude of destination point in degrees</param>
    /// <param name="destinationLongitude">Longitude of destination point in degrees</param>
    /// <returns>Distance in kilometers</returns>
    double CalculateDistanceKm(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude);
}

/// <summary>
/// Represents a hotel search result with calculated distance.
/// </summary>
/// <param name="Hotel">The hotel entity</param>
/// <param name="DistanceKm">Distance from the search location in kilometers</param>
public sealed record HotelSearchResultItem(Hotel Hotel, double DistanceKm);

/// <summary>
/// Represents a paged search result.
/// </summary>
/// <param name="Items">Hotels on the current page</param>
/// <param name="TotalCount">Total number of hotels matching the search criteria</param>
/// <param name="Page">Current page number</param>
/// <param name="PageSize">Number of items per page</param>
/// <param name="TotalPages">Total number of pages</param>
public sealed record HotelSearchPagedResult(
    IReadOnlyList<HotelSearchResultItem> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
