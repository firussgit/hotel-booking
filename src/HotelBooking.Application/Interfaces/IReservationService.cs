using HotelBooking.Application.Dtos.Reservations;

namespace HotelBooking.Application.Interfaces;

public interface IReservationService
{
    Task<ReservationDto> CreateAsync(string userId, CreateReservationDto dto, CancellationToken cancellationToken = default);
    Task<List<ReservationDto>> GetForGuestAsync(string userId, CancellationToken cancellationToken = default);
    Task<ReservationDto> GetByIdAsync(string userId, bool isAdmin, int reservationId, CancellationToken cancellationToken = default);
    Task CancelAsync(string userId, bool isAdmin, int reservationId, CancellationToken cancellationToken = default);
    Task<List<ReservationDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
