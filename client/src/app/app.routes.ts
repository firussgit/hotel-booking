import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/register/register.component').then((m) => m.RegisterComponent)
  },
  {
    path: 'book/:roomId',
    loadComponent: () => import('./features/booking/booking.component').then((m) => m.BookingComponent),
    canActivate: [authGuard]
  },
  {
    path: 'my-reservations',
    loadComponent: () =>
      import('./features/my-reservations/my-reservations.component').then((m) => m.MyReservationsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin',
    loadComponent: () =>
      import('./features/admin-dashboard/admin-dashboard.component').then((m) => m.AdminDashboardComponent),
    canActivate: [adminGuard]
  },
  {
    path: 'admin/rooms',
    loadComponent: () =>
      import('./features/admin-rooms/admin-rooms.component').then((m) => m.AdminRoomsComponent),
    canActivate: [adminGuard]
  },
  { path: '**', redirectTo: '' }
];
