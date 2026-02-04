namespace HotelSearch.Application.DTOs;

public sealed record HotelSearchResultDto(
    Guid Id,
    string Name,
    decimal PricePerNight,
    double DistanceKm);
