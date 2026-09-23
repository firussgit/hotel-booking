import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { CreateRoomRequest, Room, RoomAvailability, UpdateRoomRequest } from '../models/room.models';

@Injectable({ providedIn: 'root' })
export class RoomService {
  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Room[]> {
    return this.http.get<Room[]>(`${API_BASE_URL}/rooms`);
  }

  getById(id: number): Observable<Room> {
    return this.http.get<Room>(`${API_BASE_URL}/rooms/${id}`);
  }

  searchAvailability(checkIn: string, checkOut: string, guests: number): Observable<RoomAvailability[]> {
    const params = new HttpParams().set('checkIn', checkIn).set('checkOut', checkOut).set('guests', guests);
    return this.http.get<RoomAvailability[]>(`${API_BASE_URL}/rooms/availability`, { params });
  }

  create(request: CreateRoomRequest): Observable<Room> {
    return this.http.post<Room>(`${API_BASE_URL}/rooms`, request);
  }

  update(id: number, request: UpdateRoomRequest): Observable<Room> {
    return this.http.put<Room>(`${API_BASE_URL}/rooms/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE_URL}/rooms/${id}`);
  }
}
