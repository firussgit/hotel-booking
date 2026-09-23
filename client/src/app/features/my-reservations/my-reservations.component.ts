import { Component, OnInit, signal } from '@angular/core';
import { Reservation } from '../../core/models/reservation.models';
import { ReservationService } from '../../core/services/reservation.service';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [],
  templateUrl: './my-reservations.component.html',
  styleUrl: './my-reservations.component.css'
})
export class MyReservationsComponent implements OnInit {
  readonly reservations = signal<Reservation[]>([]);
  readonly loading = signal(true);
  readonly cancellingId = signal<number | null>(null);

  constructor(private readonly reservationService: ReservationService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.reservationService.getMine().subscribe({
      next: (reservations) => {
        this.reservations.set(reservations);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  cancel(reservation: Reservation): void {
    this.cancellingId.set(reservation.id);
    this.reservationService.cancel(reservation.id).subscribe({
      next: () => {
        this.cancellingId.set(null);
        this.load();
      },
      error: () => this.cancellingId.set(null)
    });
  }

  statusClass(status: string): string {
    return status.toLowerCase();
  }

  canCancel(status: string): boolean {
    return status === 'Pending' || status === 'Confirmed';
  }
}
