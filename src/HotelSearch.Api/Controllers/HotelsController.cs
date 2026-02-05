using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.DTOs;
using HotelSearch.Application.Hotels.Commands.CreateHotel;
using HotelSearch.Application.Hotels.Commands.DeleteHotel;
using HotelSearch.Application.Hotels.Commands.UpdateHotel;
using HotelSearch.Application.Hotels.Queries.GetAllHotels;
using HotelSearch.Application.Hotels.Queries.GetHotelById;
using Microsoft.AspNetCore.Mvc;

namespace HotelSearch.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HotelsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public HotelsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateHotelRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateHotelCommand(
            request.Name,
            request.PricePerNight,
            request.Latitude,
            request.Longitude);

        var hotel = await _dispatcher.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = hotel.Id }, hotel);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetHotelByIdQuery(id);
        var hotel = await _dispatcher.Send(query, cancellationToken);

        if (hotel is null)
        {
            return NotFound();
        }

        return Ok(hotel);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HotelDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllHotelsQuery();
        var hotels = await _dispatcher.Send(query, cancellationToken);

        return Ok(hotels);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHotelRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateHotelCommand(
            id,
            request.Name,
            request.PricePerNight,
            request.Latitude,
            request.Longitude);

        var hotel = await _dispatcher.Send(command, cancellationToken);

        return Ok(hotel);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteHotelCommand(id);
        await _dispatcher.Send(command, cancellationToken);

        return NoContent();
    }
}

public record CreateHotelRequest(string Name, decimal PricePerNight, double Latitude, double Longitude);
public record UpdateHotelRequest(string Name, decimal PricePerNight, double Latitude, double Longitude);
public record ErrorResponse(string Message, string[] Errors, string CorrelationId);
