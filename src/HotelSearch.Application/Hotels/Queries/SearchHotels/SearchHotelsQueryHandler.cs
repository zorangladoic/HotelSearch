using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;
using HotelSearch.Application.Interfaces;
using HotelSearch.Domain.Entities;
using HotelSearch.Domain.ValueObjects;

namespace HotelSearch.Application.Hotels.Queries.SearchHotels;

public sealed class SearchHotelsQueryHandler : IRequestHandler<SearchHotelsQuery, PagedResultDto<HotelSearchResultDto>>
{
    private readonly IHotelRepository _hotelRepository;
    private const double PriceWeight = 0.5;
    private const double DistanceWeight = 0.5;

    public SearchHotelsQueryHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<PagedResultDto<HotelSearchResultDto>> Handle(
        SearchHotelsQuery request,
        CancellationToken cancellationToken)
    {
        var userLocation = GeoLocation.Create(request.Latitude, request.Longitude);
        var allHotels = await _hotelRepository.GetAllAsync(cancellationToken);

        if (allHotels.Count == 0)
        {
            return new PagedResultDto<HotelSearchResultDto>(
                Items: [],
                Page: request.Page,
                PageSize: request.PageSize,
                TotalCount: 0,
                TotalPages: 0);
        }

        var hotelsWithDistance = allHotels
            .Select(h => (Hotel: h, Distance: h.DistanceTo(userLocation)))
            .ToList();

        var sortedHotels = SortByPriceAndDistance(hotelsWithDistance);

        var totalCount = sortedHotels.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var pagedHotels = sortedHotels
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(h => new HotelSearchResultDto(
                h.Hotel.Id,
                h.Hotel.Name,
                h.Hotel.PricePerNight,
                Math.Round(h.Distance, 2)))
            .ToList();

        return new PagedResultDto<HotelSearchResultDto>(
            Items: pagedHotels,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount,
            TotalPages: totalPages);
    }

    private static List<(Hotel Hotel, double Distance)> SortByPriceAndDistance(
        List<(Hotel Hotel, double Distance)> hotelsWithDistance)
    {
        if (hotelsWithDistance.Count <= 1)
        {
            return hotelsWithDistance;
        }

        var minPrice = hotelsWithDistance.Min(h => h.Hotel.PricePerNight);
        var maxPrice = hotelsWithDistance.Max(h => h.Hotel.PricePerNight);
        var priceRange = maxPrice - minPrice;

        var minDistance = hotelsWithDistance.Min(h => h.Distance);
        var maxDistance = hotelsWithDistance.Max(h => h.Distance);
        var distanceRange = maxDistance - minDistance;

        return hotelsWithDistance
            .Select(h =>
            {
                var normalizedPrice = priceRange > 0
                    ? (double)((h.Hotel.PricePerNight - minPrice) / priceRange)
                    : 0;

                var normalizedDistance = distanceRange > 0
                    ? (h.Distance - minDistance) / distanceRange
                    : 0;

                var score = (normalizedPrice * PriceWeight) + (normalizedDistance * DistanceWeight);

                return (h.Hotel, h.Distance, Score: score);
            })
            .OrderBy(h => h.Score)
            .Select(h => (h.Hotel, h.Distance))
            .ToList();
    }
}
