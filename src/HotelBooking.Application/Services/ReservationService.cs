using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Dtos.Reservations;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using HotelBooking.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IApplicationDbContext _context;
    private readonly IAvailabilityService _availabilityService;

    public ReservationService(IApplicationDbContext context, IAvailabilityService availabilityService)
    {
        _context = context;
        _availabilityService = availabilityService;
    }

    public async Task<ReservationDto> CreateAsync(string userId, CreateReservationDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.CheckIn >= dto.CheckOut)
        {
            throw new ValidationAppException("Check-out date must be after check-in date.");
        }

        if (dto.CheckIn < DateOnly.FromDateTime(DateTime.UtcNow.Date))
        {
            throw new ValidationAppException("Check-in date cannot be in the past.");
        }

        if (dto.GuestsCount <= 0)
        {
            throw new ValidationAppException("Guest count must be at least 1.");
        }

        var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Guest profile not found for the current user.");

        var room = await _context.Rooms
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == dto.RoomId, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), dto.RoomId);

        if (dto.GuestsCount > room.RoomType.MaxGuests)
        {
            throw new ValidationAppException($"Room {room.RoomNumber} accommodates at most {room.RoomType.MaxGuests} guest(s).");
        }

        if (room.Status != RoomStatus.Available)
        {
            throw new ConflictException("ROOM_NOT_AVAILABLE", $"Room {room.RoomNumber} is not available for booking.");
        }

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        // Re-check availability inside the transaction: the initial search result may be stale.
        var stillAvailable = await _availabilityService.IsRoomAvailableAsync(dto.RoomId, dto.CheckIn, dto.CheckOut, cancellationToken: cancellationToken);
        if (!stillAvailable)
        {
            throw new ConflictException("ROOM_NOT_AVAILABLE", $"Room {room.RoomNumber} is no longer available for the selected dates.");
        }

        var breakdown = PriceCalculator.Calculate(room.RoomType.BasePrice, dto.CheckIn, dto.CheckOut);

        var reservation = new Reservation
        {
            GuestId = guest.Id,
            RoomId = room.Id,
            CheckIn = dto.CheckIn,
            CheckOut = dto.CheckOut,
            GuestsCount = dto.GuestsCount,
            Status = ReservationStatus.Confirmed,
            TotalPrice = breakdown.Total,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);

        var payment = new Payment
        {
            ReservationId = reservation.Id,
            Amount = breakdown.Total,
            Status = PaymentStatus.Paid,
            TransactionReference = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return ToDto(reservation, room);
    }

    public async Task<List<ReservationDto>> GetForGuestAsync(string userId, CancellationToken cancellationToken = default)
    {
        var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Guest profile not found for the current user.");

        var reservations = await _context.Reservations
            .Include(r => r.Room).ThenInclude(room => room.RoomType)
            .Where(r => r.GuestId == guest.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reservations.Select(r => ToDto(r, r.Room)).ToList();
    }

    public async Task<List<ReservationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var reservations = await _context.Reservations
            .Include(r => r.Room).ThenInclude(room => room.RoomType)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reservations.Select(r => ToDto(r, r.Room)).ToList();
    }

    public async Task<ReservationDto> GetByIdAsync(string userId, bool isAdmin, int reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Room).ThenInclude(room => room.RoomType)
            .Include(r => r.Guest)
            .FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), reservationId);

        if (!isAdmin && reservation.Guest.UserId != userId)
        {
            throw new NotFoundException(nameof(Reservation), reservationId);
        }

        return ToDto(reservation, reservation.Room);
    }

    public async Task CancelAsync(string userId, bool isAdmin, int reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Guest)
            .FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), reservationId);

        if (!isAdmin && reservation.Guest.UserId != userId)
        {
            throw new NotFoundException(nameof(Reservation), reservationId);
        }

        if (reservation.Status is ReservationStatus.Cancelled or ReservationStatus.Completed)
        {
            throw new ConflictException("RESERVATION_NOT_CANCELLABLE", $"Reservation {reservation.Id} cannot be cancelled from status '{reservation.Status}'.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static ReservationDto ToDto(Reservation r, Room room) => new(
        r.Id,
        r.RoomId,
        room.RoomNumber,
        room.RoomType.Name,
        r.CheckIn,
        r.CheckOut,
        r.GuestsCount,
        r.Status.ToString(),
        r.TotalPrice,
        r.CreatedAt);
}
