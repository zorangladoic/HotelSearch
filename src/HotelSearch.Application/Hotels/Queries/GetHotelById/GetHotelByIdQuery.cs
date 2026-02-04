using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;

namespace HotelSearch.Application.Hotels.Queries.GetHotelById;

public sealed record GetHotelByIdQuery(Guid Id) : IRequest<HotelDto?>;
