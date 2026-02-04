using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;

namespace HotelSearch.Application.Hotels.Commands.CreateHotel;

public sealed record CreateHotelCommand(
    string Name,
    decimal PricePerNight,
    double Latitude,
    double Longitude) : IRequest<HotelDto>;
