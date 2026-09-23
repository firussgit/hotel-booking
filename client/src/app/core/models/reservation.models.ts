export interface Reservation {
  id: number;
  roomId: number;
  roomNumber: string;
  roomType: string;
  checkIn: string;
  checkOut: string;
  guestsCount: number;
  status: string;
  totalPrice: number;
  createdAt: string;
}

export interface CreateReservationRequest {
  roomId: number;
  checkIn: string;
  checkOut: string;
  guestsCount: number;
}
