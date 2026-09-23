using HotelBooking.Application.Dtos.Rooms;

namespace HotelBooking.Application.Interfaces;

public interface IAvailabilityService
{
    Task<List<RoomAvailabilityDto>> SearchAsync(DateOnly checkIn, DateOnly checkOut, int guests, CancellationToken cancellationToken = default);

    Task<bool> IsRoomAvailableAsync(int roomId, DateOnly checkIn, DateOnly checkOut, int? excludeReservationId = null, CancellationToken cancellationToken = default);
}
