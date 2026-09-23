export interface Room {
  id: number;
  hotelId: number;
  roomNumber: string;
  floor: number;
  status: string;
  roomTypeId: number;
  roomTypeName: string;
  maxGuests: number;
  pricePerNight: number;
}

export interface RoomAvailability {
  roomId: number;
  roomNumber: string;
  roomType: string;
  maxGuests: number;
  pricePerNight: number;
  totalPrice: number;
}

export interface CreateRoomRequest {
  hotelId: number;
  roomTypeId: number;
  roomNumber: string;
  floor: number;
}

export interface UpdateRoomRequest {
  roomTypeId: number;
  roomNumber: string;
  floor: number;
  status: string;
}
