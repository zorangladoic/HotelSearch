namespace HotelSearch.Domain.Exceptions;

public class HotelNotFoundException : DomainException
{
    public Guid HotelId { get; }

    public HotelNotFoundException(Guid hotelId)
        : base($"Hotel with ID '{hotelId}' was not found.")
    {
        HotelId = hotelId;
    }
}
