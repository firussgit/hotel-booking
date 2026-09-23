import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiError } from '../../core/models/api-error';
import { Room } from '../../core/models/room.models';
import { ReservationService } from '../../core/services/reservation.service';
import { RoomService } from '../../core/services/room.service';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [],
  templateUrl: './booking.component.html',
  styleUrl: './booking.component.css'
})
export class BookingComponent implements OnInit {
  roomId!: number;
  checkIn = '';
  checkOut = '';
  guests = 1;
  nights = 0;

  readonly room = signal<Room | null>(null);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly roomService: RoomService,
    private readonly reservationService: ReservationService
  ) {}

  ngOnInit(): void {
    this.roomId = Number(this.route.snapshot.paramMap.get('roomId'));
    const query = this.route.snapshot.queryParamMap;
    this.checkIn = query.get('checkIn') ?? '';
    this.checkOut = query.get('checkOut') ?? '';
    this.guests = Number(query.get('guests') ?? 1);

    if (this.checkIn && this.checkOut) {
      const msPerDay = 1000 * 60 * 60 * 24;
      this.nights = Math.round((new Date(this.checkOut).getTime() - new Date(this.checkIn).getTime()) / msPerDay);
    }

    this.roomService.getById(this.roomId).subscribe({
      next: (room) => {
        this.room.set(room);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Room not found.');
        this.loading.set(false);
      }
    });
  }

  get totalPrice(): number {
    const room = this.room();
    return room ? room.pricePerNight * this.nights * 1.1 : 0;
  }

  confirm(): void {
    this.errorMessage.set(null);
    this.submitting.set(true);

    this.reservationService
      .create({ roomId: this.roomId, checkIn: this.checkIn, checkOut: this.checkOut, guestsCount: this.guests })
      .subscribe({
        next: () => this.router.navigate(['/my-reservations']),
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          const apiError = err.error as ApiError | undefined;
          this.errorMessage.set(apiError?.message ?? 'Booking failed.');
        }
      });
  }
}
