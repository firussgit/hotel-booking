namespace HotelBooking.Application.Dtos.Rooms;

public record RoomAvailabilityDto(
    int RoomId,
    string RoomNumber,
    string RoomType,
    int MaxGuests,
    decimal PricePerNight,
    decimal TotalPrice);
