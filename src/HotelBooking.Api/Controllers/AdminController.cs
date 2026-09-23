using HotelBooking.Application.Common.Constants;
using HotelBooking.Application.Dtos.Admin;
using HotelBooking.Application.Dtos.Reservations;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly IDashboardService _dashboardService;

    public AdminController(IReservationService reservationService, IDashboardService dashboardService)
    {
        _reservationService = reservationService;
        _dashboardService = dashboardService;
    }

    [HttpGet("reservations")]
    public async Task<ActionResult<List<ReservationDto>>> GetAllReservations(CancellationToken cancellationToken)
        => Ok(await _reservationService.GetAllAsync(cancellationToken));

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken cancellationToken)
        => Ok(await _dashboardService.GetDashboardAsync(cancellationToken));
}
