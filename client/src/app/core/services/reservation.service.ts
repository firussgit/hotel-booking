import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { CreateReservationRequest, Reservation } from '../models/reservation.models';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  constructor(private readonly http: HttpClient) {}

  create(request: CreateReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(`${API_BASE_URL}/reservations`, request);
  }

  getMine(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(`${API_BASE_URL}/reservations`);
  }

  cancel(id: number): Observable<void> {
    return this.http.post<void>(`${API_BASE_URL}/reservations/${id}/cancel`, {});
  }
}
