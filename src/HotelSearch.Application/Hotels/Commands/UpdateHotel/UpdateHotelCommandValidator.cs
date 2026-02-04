using FluentValidation;
using HotelSearch.Domain.Entities;

namespace HotelSearch.Application.Hotels.Commands.UpdateHotel;

public sealed class UpdateHotelCommandValidator : AbstractValidator<UpdateHotelCommand>
{
    public UpdateHotelCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hotel name is required.")
            .MaximumLength(Hotel.MaxNameLength).WithMessage($"Hotel name cannot exceed {Hotel.MaxNameLength} characters.");

        RuleFor(x => x.PricePerNight)
            .GreaterThanOrEqualTo(Hotel.MinPrice).WithMessage($"Price per night must be at least {Hotel.MinPrice}.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.");
    }
}
