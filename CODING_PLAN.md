# Hotel Booking System — Coding Plan

Derived from [PLAN.md](PLAN.md). Check items off as we implement them, phase by phase.

## Phase 1 — Foundation
- [x] Create solution (`HotelBooking.sln`) with projects: Api, Application, Domain, Infrastructure, Tests
- [x] Configure project references (Api → Application/Infrastructure; Infrastructure → Application/Domain; Application → Domain)
- [x] Add EF Core packages, configure `DbContext`
- [x] Configure SQLite connection
- [x] Create initial migration (empty — populated once Phase 2 entities land)
- [x] Seed sample hotel/rooms data

## Phase 2 — Core domain
- [x] `Hotel` entity
- [x] `RoomType` entity
- [x] `Room` entity (with `Status` enum: Available/Maintenance/Inactive)
- [x] `Guest` entity
- [x] `Reservation` entity (with `Status` enum: Pending/Confirmed/Cancelled/Completed)
- [x] `Payment` entity (with `Status` enum: Pending/Paid/Failed/Refunded)
- [x] Configure entity relationships & EF Core mappings

## Phase 3 — API
- [x] Auth endpoints: `POST /api/auth/register`, `POST /api/auth/login` (ASP.NET Core Identity / JWT)
- [x] Room endpoints: `GET/POST/PUT/DELETE /api/rooms`, `GET /api/rooms/{id}`
- [x] Availability endpoint: `GET /api/rooms/availability`
- [x] Reservation endpoints: `POST/GET /api/reservations`, `GET /api/reservations/{id}`, `POST /api/reservations/{id}/cancel`
- [x] Admin endpoints: `GET /api/admin/reservations`, `GET /api/admin/dashboard` (admin room mutation reuses the `/api/rooms` endpoints, gated by `[Authorize(Roles = Admin)]`, instead of duplicating CRUD under `/api/admin/rooms`)
- [x] Role-based authorization (Guest vs Admin)

## Phase 4 — Business logic
- [x] `AvailabilityService` — overlap detection logic
- [x] Date validation (check-in/check-out)
- [x] Guest count vs room capacity validation
- [x] `ReservationService` — price calculation (subtotal + tax)
- [x] Cancellation flow
- [x] Double-booking protection (re-check availability inside transaction at reservation creation)
- [x] Consistent API error responses (status/code/message) + proper HTTP status codes

## Phase 5 — Tests
- [x] Unit tests: `CalculateReservationPrice`, `ShouldRejectInvalidDates`, `ShouldRejectMoreGuestsThanRoomCapacity`
- [x] Unit tests: `ShouldDetectOverlappingReservation`, `ShouldAllowAdjacentReservations`
- [x] Integration tests: reservation + payment creation via API
- [x] Integration/concurrency test: double-booking prevention (verified with real concurrent HTTP requests, not just sequential calls; 22/22 tests passing, re-run 3x to confirm no flakiness)

## Phase 6 — Frontend (Angular)
- [ ] Home / search page
- [ ] Search results page
- [ ] Booking page
- [ ] My reservations page
- [ ] Admin dashboard

## Phase 7 — Polish
- [x] Global exception handling middleware (done in Phase 3/4)
- [x] Structured logging (Serilog), no sensitive data logged
- [x] Swagger/OpenAPI docs (done in Phase 3; now enabled in all environments)
- [ ] Docker + docker-compose — API + SQLite (no separate Postgres/Angular containers per our stack choices); **files written but unverified**, Docker Desktop install is blocked on an admin/UAC prompt this session can't answer — user installing manually, verify once available
- [x] README (overview, architecture, API docs, design decisions, running locally, Docker)
- [ ] Screenshots (blocked on Phase 6 frontend existing)
