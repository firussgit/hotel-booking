using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Dtos.Admin;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;

    public DashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var totalRooms = await _context.Rooms.CountAsync(cancellationToken);

        var occupiedRooms = await _context.Reservations
            .Where(r => (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
                && r.CheckIn <= today && r.CheckOut > today)
            .Select(r => r.RoomId)
            .Distinct()
            .CountAsync(cancellationToken);

        var paidAmounts = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .Select(p => p.Amount)
            .ToListAsync(cancellationToken);
        var totalRevenue = paidAmounts.Sum();

        var occupancyRate = totalRooms == 0 ? 0 : Math.Round(occupiedRooms * 100.0 / totalRooms, 1);

        return new DashboardDto(totalRooms, occupancyRate, totalRevenue);
    }
}
