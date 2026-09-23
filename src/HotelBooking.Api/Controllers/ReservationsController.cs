using System.Security.Claims;
using HotelBooking.Application.Dtos.Reservations;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Authenticated request is missing a user id claim.");

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(CreateReservationDto dto, CancellationToken cancellationToken)
    {
        var reservation = await _reservationService.CreateAsync(UserId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    [HttpGet]
    public async Task<ActionResult<List<ReservationDto>>> GetMine(CancellationToken cancellationToken)
        => Ok(await _reservationService.GetForGuestAsync(UserId, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationDto>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _reservationService.GetByIdAsync(UserId, User.IsInRole("Admin"), id, cancellationToken));

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _reservationService.CancelAsync(UserId, User.IsInRole("Admin"), id, cancellationToken);
        return NoContent();
    }
}
