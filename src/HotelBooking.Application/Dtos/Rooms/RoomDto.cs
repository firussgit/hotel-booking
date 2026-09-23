namespace HotelBooking.Application.Dtos.Rooms;

public record RoomDto(
    int Id,
    int HotelId,
    string RoomNumber,
    int Floor,
    string Status,
    int RoomTypeId,
    string RoomTypeName,
    int MaxGuests,
    decimal PricePerNight);
