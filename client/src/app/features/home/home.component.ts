import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiError } from '../../core/models/api-error';
import { RoomAvailability } from '../../core/models/room.models';
import { AuthService } from '../../core/services/auth.service';
import { RoomService } from '../../core/services/room.service';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

function inDaysIso(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() + days);
  return date.toISOString().slice(0, 10);
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  checkIn = todayIso();
  checkOut = inDaysIso(3);
  guests = 2;

  readonly searching = signal(false);
  readonly searched = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly results = signal<RoomAvailability[]>([]);

  constructor(
    private readonly roomService: RoomService,
    readonly auth: AuthService,
    private readonly router: Router
  ) {}

  search(): void {
    this.errorMessage.set(null);

    if (!this.checkIn || !this.checkOut || this.checkIn >= this.checkOut) {
      this.errorMessage.set('Check-out date must be after check-in date.');
      return;
    }

    this.searching.set(true);
    this.searched.set(true);

    this.roomService.searchAvailability(this.checkIn, this.checkOut, this.guests).subscribe({
      next: (rooms) => {
        this.results.set(rooms);
        this.searching.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.searching.set(false);
        const apiError = err.error as ApiError | undefined;
        this.errorMessage.set(apiError?.message ?? 'Search failed.');
      }
    });
  }

  book(room: RoomAvailability): void {
    this.router.navigate(['/book', room.roomId], {
      queryParams: { checkIn: this.checkIn, checkOut: this.checkOut, guests: this.guests }
    });
  }
}
