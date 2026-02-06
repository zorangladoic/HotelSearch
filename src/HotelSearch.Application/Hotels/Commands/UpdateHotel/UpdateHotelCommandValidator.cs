using FluentValidation;
using HotelSearch.Application.Common.Validators;
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
            .GreaterThanOrEqualTo(Hotel.MinPrice).WithMessage($"Price per night must be at least {Hotel.MinPrice}.")
            .LessThanOrEqualTo(Hotel.MaxPrice).WithMessage($"Price per night cannot exceed {Hotel.MaxPrice}.");

        RuleFor(x => x.Latitude).ValidLatitude();
        RuleFor(x => x.Longitude).ValidLongitude();
    }
}
