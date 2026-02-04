using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;

namespace HotelSearch.Application.Hotels.Queries.SearchHotels;

public sealed record SearchHotelsQuery(
    double Latitude,
    double Longitude,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResultDto<HotelSearchResultDto>>;
