using FluentValidation;
using HotelSearch.Application.Common.Validators;

namespace HotelSearch.Application.Hotels.Queries.SearchHotels;

public sealed class SearchHotelsQueryValidator : AbstractValidator<SearchHotelsQuery>
{
    public SearchHotelsQueryValidator()
    {
        RuleFor(x => x.Latitude).ValidLatitude();
        RuleFor(x => x.Longitude).ValidLongitude();

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}
