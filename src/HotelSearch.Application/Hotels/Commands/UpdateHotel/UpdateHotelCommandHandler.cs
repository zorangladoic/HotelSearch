using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;
using HotelSearch.Application.Interfaces;
using HotelSearch.Application.Mappings;
using HotelSearch.Domain.Exceptions;

namespace HotelSearch.Application.Hotels.Commands.UpdateHotel;

public sealed class UpdateHotelCommandHandler : IRequestHandler<UpdateHotelCommand, HotelDto>
{
    private readonly IHotelRepository _hotelRepository;

    public UpdateHotelCommandHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<HotelDto> Handle(UpdateHotelCommand request, CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new HotelNotFoundException(request.Id);

        hotel.Update(
            request.Name,
            request.PricePerNight,
            request.Latitude,
            request.Longitude);

        var updatedHotel = await _hotelRepository.UpdateAsync(hotel, cancellationToken);

        return updatedHotel.ToDto();
    }
}
