using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;
using HotelSearch.Application.Interfaces;
using HotelSearch.Application.Mappings;

namespace HotelSearch.Application.Hotels.Queries.GetHotelById;

public sealed class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, HotelDto?>
{
    private readonly IHotelRepository _hotelRepository;

    public GetHotelByIdQueryHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<HotelDto?> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdAsync(request.Id, cancellationToken);

        return hotel?.ToDto();
    }
}
