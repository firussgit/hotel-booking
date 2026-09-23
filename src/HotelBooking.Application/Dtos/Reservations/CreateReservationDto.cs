namespace HotelBooking.Application.Dtos.Reservations;

public record CreateReservationDto(int RoomId, DateOnly CheckIn, DateOnly CheckOut, int GuestsCount);
