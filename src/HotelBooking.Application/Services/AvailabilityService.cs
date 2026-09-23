using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Dtos.Rooms;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Enums;
using HotelBooking.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private static readonly ReservationStatus[] BlockingStatuses =
    {
        ReservationStatus.Pending,
        ReservationStatus.Confirmed
    };

    private readonly IApplicationDbContext _context;

    public AvailabilityService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoomAvailabilityDto>> SearchAsync(DateOnly checkIn, DateOnly checkOut, int guests, CancellationToken cancellationToken = default)
    {
        var candidateRooms = await _context.Rooms
            .Include(r => r.RoomType)
            .Where(r => r.Status == RoomStatus.Available && r.RoomType.MaxGuests >= guests)
            .ToListAsync(cancellationToken);

        var roomIds = candidateRooms.Select(r => r.Id).ToList();

        var overlappingRoomIds = await _context.Reservations
            .Where(res => roomIds.Contains(res.RoomId)
                && BlockingStatuses.Contains(res.Status)
                && res.CheckIn < checkOut && res.CheckOut > checkIn)
            .Select(res => res.RoomId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var overlapping = overlappingRoomIds.ToHashSet();

        return candidateRooms
            .Where(r => !overlapping.Contains(r.Id))
            .Select(r =>
            {
                var breakdown = PriceCalculator.Calculate(r.RoomType.BasePrice, checkIn, checkOut);
                return new RoomAvailabilityDto(
                    r.Id,
                    r.RoomNumber,
                    r.RoomType.Name,
                    r.RoomType.MaxGuests,
                    r.RoomType.BasePrice,
                    breakdown.Total);
            })
            .ToList();
    }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateOnly checkIn, DateOnly checkOut, int? excludeReservationId = null, CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);
        if (room is null || room.Status != RoomStatus.Available)
        {
            return false;
        }

        var query = _context.Reservations.Where(res => res.RoomId == roomId
            && BlockingStatuses.Contains(res.Status)
            && res.CheckIn < checkOut && res.CheckOut > checkIn);

        if (excludeReservationId is not null)
        {
            query = query.Where(res => res.Id != excludeReservationId);
        }

        return !await query.AnyAsync(cancellationToken);
    }
}
