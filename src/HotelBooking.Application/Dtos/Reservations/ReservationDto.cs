namespace HotelBooking.Application.Dtos.Reservations;

public record ReservationDto(
    int Id,
    int RoomId,
    string RoomNumber,
    string RoomType,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int GuestsCount,
    string Status,
    decimal TotalPrice,
    DateTime CreatedAt);
