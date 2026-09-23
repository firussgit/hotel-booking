namespace HotelBooking.Application.Dtos.Rooms;

public record CreateRoomDto(int HotelId, int RoomTypeId, string RoomNumber, int Floor);
