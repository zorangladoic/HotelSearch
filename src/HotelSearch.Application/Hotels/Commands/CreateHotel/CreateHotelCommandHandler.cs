using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;
using HotelSearch.Application.Interfaces;
using HotelSearch.Application.Mappings;
using HotelSearch.Domain.Entities;

namespace HotelSearch.Application.Hotels.Commands.CreateHotel;

public sealed class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, HotelDto>
{
    private readonly IHotelRepository _hotelRepository;

    public CreateHotelCommandHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<HotelDto> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        var hotel = Hotel.Create(
            request.Name,
            request.PricePerNight,
            request.Latitude,
            request.Longitude);

        var createdHotel = await _hotelRepository.AddAsync(hotel, cancellationToken);

        return createdHotel.ToDto();
    }
}
