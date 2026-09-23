using HotelBooking.Application.Common.Constants;
using HotelBooking.Application.Dtos.Rooms;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IAvailabilityService _availabilityService;

    public RoomsController(IRoomService roomService, IAvailabilityService availabilityService)
    {
        _roomService = roomService;
        _availabilityService = availabilityService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoomDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _roomService.GetAllAsync(cancellationToken));

    [HttpGet("availability")]
    public async Task<ActionResult<List<RoomAvailabilityDto>>> GetAvailability(
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        [FromQuery] int guests,
        CancellationToken cancellationToken)
    {
        if (checkIn >= checkOut)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "INVALID_DATE_RANGE", detail: "checkOut must be after checkIn.");
        }

        if (guests <= 0)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "INVALID_GUEST_COUNT", detail: "guests must be at least 1.");
        }

        var results = await _availabilityService.SearchAsync(checkIn, checkOut, guests, cancellationToken);
        return Ok(results);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomDto>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _roomService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<RoomDto>> Create(CreateRoomDto dto, CancellationToken cancellationToken)
    {
        var room = await _roomService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<RoomDto>> Update(int id, UpdateRoomDto dto, CancellationToken cancellationToken)
        => Ok(await _roomService.UpdateAsync(id, dto, cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _roomService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
