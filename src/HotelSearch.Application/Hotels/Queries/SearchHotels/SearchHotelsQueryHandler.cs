using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;
using HotelSearch.Application.Interfaces;

namespace HotelSearch.Application.Hotels.Queries.SearchHotels;

/// <summary>
/// Handles search queries by orchestrating between the repository and search service.
///
/// Separation of Concerns:
/// - Repository: Pure data access (CRUD operations)
/// - SearchService: Business logic (bounding box filtering, distance calculation, sorting)
/// - Handler: Orchestration and DTO mapping
/// </summary>
public sealed class SearchHotelsQueryHandler : IRequestHandler<SearchHotelsQuery, PagedResultDto<HotelSearchResultDto>>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IHotelSearchService _searchService;

    public SearchHotelsQueryHandler(
        IHotelRepository hotelRepository,
        IHotelSearchService searchService)
    {
        _hotelRepository = hotelRepository;
        _searchService = searchService;
    }

    public async Task<PagedResultDto<HotelSearchResultDto>> Handle(
        SearchHotelsQuery request,
        CancellationToken cancellationToken)
    {
        // Get all hotels from repository
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

        // Use search service for filtering and sorting
        // No radius filter for global search - searches all hotels
        var searchResults = _searchService.Search(
            allHotels,
            request.Latitude,
            request.Longitude,
            radiusKm: null);

        var totalCount = searchResults.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // Apply pagination
        var pagedResults = searchResults
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new HotelSearchResultDto(
                item.Hotel.Id,
                item.Hotel.Name,
                item.Hotel.PricePerNight,
                Math.Round(item.DistanceKm, 2)))
            .ToList();

        return new PagedResultDto<HotelSearchResultDto>(
            Items: pagedResults,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount,
            TotalPages: totalPages);
    }
}
