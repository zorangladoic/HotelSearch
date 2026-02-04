using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;

namespace HotelSearch.Application.Hotels.Queries.GetAllHotels;

public sealed record GetAllHotelsQuery : IRequest<IReadOnlyList<HotelDto>>;
