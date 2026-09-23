# Hotel Booking System — Coding Plan

Derived from [PLAN.md](PLAN.md). Check items off as we implement them, phase by phase.

## Phase 1 — Foundation
- [x] Create solution (`HotelBooking.sln`) with projects: Api, Application, Domain, Infrastructure, Tests
- [x] Configure project references (Api → Application/Infrastructure; Infrastructure → Application/Domain; Application → Domain)
- [x] Add EF Core packages, configure `DbContext`
- [x] Configure SQLite connection
- [x] Create initial migration (empty — populated once Phase 2 entities land)
- [ ] Seed sample hotel/rooms data (after Phase 2 entities exist)

## Phase 2 — Core domain
- [ ] `Hotel` entity
- [ ] `RoomType` entity
- [ ] `Room` entity (with `Status` enum: Available/Maintenance/Inactive)
- [ ] `Guest` entity
- [ ] `Reservation` entity (with `Status` enum: Pending/Confirmed/Cancelled/Completed)
- [ ] `Payment` entity (with `Status` enum: Pending/Paid/Failed/Refunded)
- [ ] Configure entity relationships & EF Core mappings

## Phase 3 — API
- [ ] Auth endpoints: `POST /api/auth/register`, `POST /api/auth/login` (ASP.NET Core Identity / JWT)
- [ ] Room endpoints: `GET/POST/PUT/DELETE /api/rooms`, `GET /api/rooms/{id}`
- [ ] Availability endpoint: `GET /api/rooms/availability`
- [ ] Reservation endpoints: `POST/GET /api/reservations`, `GET /api/reservations/{id}`, `POST /api/reservations/{id}/cancel`
- [ ] Admin endpoints: `GET /api/admin/reservations`, `GET /api/admin/dashboard`, `POST/PUT /api/admin/rooms`
- [ ] Role-based authorization (Guest vs Admin)

## Phase 4 — Business logic
- [ ] `AvailabilityService` — overlap detection logic
- [ ] Date validation (check-in/check-out)
- [ ] Guest count vs room capacity validation
- [ ] `ReservationService` — price calculation (subtotal + tax)
- [ ] Cancellation flow
- [ ] Double-booking protection (re-check availability inside transaction at reservation creation)
- [ ] Consistent API error responses (status/code/message) + proper HTTP status codes

## Phase 5 — Tests
- [ ] Unit tests: `CalculateReservationPrice`, `ShouldRejectInvalidDates`, `ShouldRejectMoreGuestsThanRoomCapacity`
- [ ] Unit tests: `ShouldDetectOverlappingReservation`, `ShouldAllowAdjacentReservations`
- [ ] Integration tests: reservation + payment creation via API
- [ ] Integration/concurrency test: double-booking prevention

## Phase 6 — Frontend (Angular)
- [ ] Home / search page
- [ ] Search results page
- [ ] Booking page
- [ ] My reservations page
- [ ] Admin dashboard

## Phase 7 — Polish
- [ ] Global exception handling middleware
- [ ] Structured logging (Serilog), no sensitive data logged
- [ ] Swagger/OpenAPI docs
- [ ] Docker + docker-compose (Angular, API, PostgreSQL)
- [ ] README (overview, architecture, API docs, design decisions, running locally)
- [ ] Screenshots
