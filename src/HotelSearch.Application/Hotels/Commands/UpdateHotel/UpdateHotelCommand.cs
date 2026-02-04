using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;

namespace HotelSearch.Application.Hotels.Commands.UpdateHotel;

public sealed record UpdateHotelCommand(
    Guid Id,
    string Name,
    decimal PricePerNight,
    double Latitude,
    double Longitude) : IRequest<HotelDto>;
