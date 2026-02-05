using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;
using HotelSearch.Application.Hotels.Queries.SearchHotels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HotelSearch.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public SearchController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet]
    [OutputCache(PolicyName = "SearchResults")]
    [ProducesResponseType(typeof(PagedResultDto<HotelSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] double? latitude,
        [FromQuery] double? longitude,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!latitude.HasValue)
        {
            return BadRequest(new ErrorResponse("Validation failed", ["Latitude is required."], HttpContext.TraceIdentifier));
        }

        if (!longitude.HasValue)
        {
            return BadRequest(new ErrorResponse("Validation failed", ["Longitude is required."], HttpContext.TraceIdentifier));
        }

        var query = new SearchHotelsQuery(latitude.Value, longitude.Value, page, pageSize);
        var result = await _dispatcher.Send(query, cancellationToken);

        return Ok(result);
    }
}
