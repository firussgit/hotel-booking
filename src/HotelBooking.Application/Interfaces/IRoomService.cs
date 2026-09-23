using HotelBooking.Application.Dtos.Rooms;

namespace HotelBooking.Application.Interfaces;

public interface IRoomService
{
    Task<List<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoomDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RoomDto> CreateAsync(CreateRoomDto dto, CancellationToken cancellationToken = default);
    Task<RoomDto> UpdateAsync(int id, UpdateRoomDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
