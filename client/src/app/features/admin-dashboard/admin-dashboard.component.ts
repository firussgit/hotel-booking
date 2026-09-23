import { Component, OnInit, signal } from '@angular/core';
import { Dashboard } from '../../core/models/admin.models';
import { Reservation } from '../../core/models/reservation.models';
import { AdminService } from '../../core/services/admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  readonly dashboard = signal<Dashboard | null>(null);
  readonly reservations = signal<Reservation[]>([]);
  readonly loading = signal(true);

  constructor(private readonly adminService: AdminService) {}

  ngOnInit(): void {
    this.adminService.getDashboard().subscribe((dashboard) => this.dashboard.set(dashboard));
    this.adminService.getAllReservations().subscribe({
      next: (reservations) => {
        this.reservations.set(reservations);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  statusClass(status: string): string {
    return status.toLowerCase();
  }
}
