using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;
using HotelSearch.Application.Interfaces;
using HotelSearch.Application.Mappings;

namespace HotelSearch.Application.Hotels.Queries.GetAllHotels;

public sealed class GetAllHotelsQueryHandler : IRequestHandler<GetAllHotelsQuery, IReadOnlyList<HotelDto>>
{
    private readonly IHotelRepository _hotelRepository;

    public GetAllHotelsQueryHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<IReadOnlyList<HotelDto>> Handle(GetAllHotelsQuery request, CancellationToken cancellationToken)
    {
        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);

        return hotels.ToDto();
    }
}
