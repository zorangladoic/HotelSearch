using HotelSearch.Application.Common.Interfaces;

namespace HotelSearch.Application.Hotels.Commands.DeleteHotel;

public sealed record DeleteHotelCommand(Guid Id) : IRequest<bool>;
