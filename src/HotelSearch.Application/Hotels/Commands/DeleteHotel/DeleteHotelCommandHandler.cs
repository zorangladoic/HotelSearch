using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.Interfaces;
using HotelSearch.Domain.Exceptions;

namespace HotelSearch.Application.Hotels.Commands.DeleteHotel;

public sealed class DeleteHotelCommandHandler : IRequestHandler<DeleteHotelCommand, bool>
{
    private readonly IHotelRepository _hotelRepository;

    public DeleteHotelCommandHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<bool> Handle(DeleteHotelCommand request, CancellationToken cancellationToken)
    {
        var exists = await _hotelRepository.ExistsAsync(request.Id, cancellationToken);

        if (!exists)
        {
            throw new HotelNotFoundException(request.Id);
        }

        return await _hotelRepository.DeleteAsync(request.Id, cancellationToken);
    }
}
