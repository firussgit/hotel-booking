namespace HotelBooking.Application.Dtos.Rooms;

public record UpdateRoomDto(int RoomTypeId, string RoomNumber, int Floor, string Status);
