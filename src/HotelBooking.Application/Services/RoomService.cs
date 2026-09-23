using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Dtos.Rooms;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Services;

public class RoomService : IRoomService
{
    private readonly IApplicationDbContext _context;

    public RoomService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rooms = await _context.Rooms
            .Include(r => r.RoomType)
            .ToListAsync(cancellationToken);

        return rooms.Select(ToDto).ToList();
    }

    public async Task<RoomDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), id);

        return ToDto(room);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto, CancellationToken cancellationToken = default)
    {
        var hotelExists = await _context.Hotels.AnyAsync(h => h.Id == dto.HotelId, cancellationToken);
        if (!hotelExists)
        {
            throw new NotFoundException(nameof(Hotel), dto.HotelId);
        }

        var roomTypeExists = await _context.RoomTypes.AnyAsync(rt => rt.Id == dto.RoomTypeId, cancellationToken);
        if (!roomTypeExists)
        {
            throw new NotFoundException(nameof(RoomType), dto.RoomTypeId);
        }

        var room = new Room
        {
            HotelId = dto.HotelId,
            RoomTypeId = dto.RoomTypeId,
            RoomNumber = dto.RoomNumber,
            Floor = dto.Floor,
            Status = RoomStatus.Available
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(room.Id, cancellationToken);
    }

    public async Task<RoomDto> UpdateAsync(int id, UpdateRoomDto dto, CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), id);

        if (!Enum.TryParse<RoomStatus>(dto.Status, ignoreCase: true, out var status))
        {
            throw new ValidationAppException($"Invalid room status '{dto.Status}'.");
        }

        var roomTypeExists = await _context.RoomTypes.AnyAsync(rt => rt.Id == dto.RoomTypeId, cancellationToken);
        if (!roomTypeExists)
        {
            throw new NotFoundException(nameof(RoomType), dto.RoomTypeId);
        }

        room.RoomTypeId = dto.RoomTypeId;
        room.RoomNumber = dto.RoomNumber;
        room.Floor = dto.Floor;
        room.Status = status;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(room.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), id);

        var hasActiveReservations = await _context.Reservations.AnyAsync(
            res => res.RoomId == id && (res.Status == ReservationStatus.Pending || res.Status == ReservationStatus.Confirmed),
            cancellationToken);

        if (hasActiveReservations)
        {
            throw new ConflictException("ROOM_HAS_ACTIVE_RESERVATIONS", "Cannot delete a room with active reservations.");
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static RoomDto ToDto(Room r) => new(
        r.Id,
        r.HotelId,
        r.RoomNumber,
        r.Floor,
        r.Status.ToString(),
        r.RoomTypeId,
        r.RoomType.Name,
        r.RoomType.MaxGuests,
        r.RoomType.BasePrice);
}
