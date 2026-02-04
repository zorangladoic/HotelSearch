namespace HotelSearch.Application.DTOs;

public sealed record HotelDto(
    Guid Id,
    string Name,
    decimal PricePerNight,
    double Latitude,
    double Longitude,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
