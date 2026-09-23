import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiError } from '../../core/models/api-error';
import { Room } from '../../core/models/room.models';
import { RoomService } from '../../core/services/room.service';

interface RoomTypeOption {
  id: number;
  name: string;
}

@Component({
  selector: 'app-admin-rooms',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './admin-rooms.component.html',
  styleUrl: './admin-rooms.component.css'
})
export class AdminRoomsComponent implements OnInit {
  readonly rooms = signal<Room[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly creating = signal(false);

  roomTypeOptions: RoomTypeOption[] = [];
  defaultHotelId = 1;

  newRoomNumber = '';
  newFloor = 1;
  newRoomTypeId: number | null = null;

  readonly statuses = ['Available', 'Maintenance', 'Inactive'];

  constructor(private readonly roomService: RoomService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.roomService.getAll().subscribe({
      next: (rooms) => {
        this.rooms.set(rooms);
        this.roomTypeOptions = Array.from(
          new Map(rooms.map((r) => [r.roomTypeId, { id: r.roomTypeId, name: r.roomTypeName }])).values()
        );
        if (rooms.length > 0) {
          this.defaultHotelId = rooms[0].hotelId;
        }
        if (!this.newRoomTypeId && this.roomTypeOptions.length > 0) {
          this.newRoomTypeId = this.roomTypeOptions[0].id;
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  createRoom(): void {
    if (!this.newRoomTypeId || !this.newRoomNumber) {
      return;
    }

    this.errorMessage.set(null);
    this.creating.set(true);

    this.roomService
      .create({
        hotelId: this.defaultHotelId,
        roomTypeId: this.newRoomTypeId,
        roomNumber: this.newRoomNumber,
        floor: this.newFloor
      })
      .subscribe({
        next: () => {
          this.creating.set(false);
          this.newRoomNumber = '';
          this.load();
        },
        error: (err: HttpErrorResponse) => {
          this.creating.set(false);
          const apiError = err.error as ApiError | undefined;
          this.errorMessage.set(apiError?.message ?? 'Could not create room.');
        }
      });
  }

  updateStatus(room: Room, status: string): void {
    this.roomService
      .update(room.id, { roomTypeId: room.roomTypeId, roomNumber: room.roomNumber, floor: room.floor, status })
      .subscribe({
        next: () => this.load(),
        error: (err: HttpErrorResponse) => {
          const apiError = err.error as ApiError | undefined;
          this.errorMessage.set(apiError?.message ?? 'Could not update room.');
        }
      });
  }

  deleteRoom(room: Room): void {
    this.roomService.delete(room.id).subscribe({
      next: () => this.load(),
      error: (err: HttpErrorResponse) => {
        const apiError = err.error as ApiError | undefined;
        this.errorMessage.set(apiError?.message ?? 'Could not delete room.');
      }
    });
  }
}
