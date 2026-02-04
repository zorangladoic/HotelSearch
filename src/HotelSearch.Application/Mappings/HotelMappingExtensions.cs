using HotelSearch.Application.DTOs;
using HotelSearch.Domain.Entities;

namespace HotelSearch.Application.Mappings;

public static class HotelMappingExtensions
{
    public static HotelDto ToDto(this Hotel hotel)
    {
        return new HotelDto(
            hotel.Id,
            hotel.Name,
            hotel.PricePerNight,
            hotel.Location.Latitude,
            hotel.Location.Longitude,
            hotel.CreatedAt,
            hotel.UpdatedAt);
    }

    public static IReadOnlyList<HotelDto> ToDto(this IEnumerable<Hotel> hotels)
    {
        return hotels.Select(h => h.ToDto()).ToList();
    }
}
