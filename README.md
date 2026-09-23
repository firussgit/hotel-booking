# Hotel Booking System

A small hotel management and booking platform built with ASP.NET Core and EF Core. The focus isn't CRUD — it's correctness under concurrency: overlapping reservations are rejected, availability is re-checked at booking time (not trusted from search), and double-booking is prevented with a database transaction, verified by a test that fires two real concurrent HTTP requests at the same room and dates.

## Overview

Guests can search for available rooms by date and guest count, book a room, view their reservations, and cancel them. Admins manage rooms and room types, view all reservations, and see occupancy/revenue on a dashboard. Authentication is JWT-based with two roles (`Guest`, `Admin`) enforced on every mutating endpoint.

## Features

- Guest registration/login, room search with live pricing, booking, cancellation, "my reservations"
- Admin room management (create/update/delete), all-reservations view, occupancy/revenue dashboard
- Role-based authorization — a Guest token cannot hit admin-only or other guests' endpoints
- Consistent JSON error shape (`{status, code, message}`) with proper HTTP status codes
- Structured logging (Serilog) to console + rolling file, with reservation/payment/rejection events logged
- 22 automated tests covering the pure business rules and the full HTTP flow, including a concurrency test

## Architecture

```
HotelBooking/
├── HotelBooking.Api             ASP.NET Core Web API — controllers, JWT/Swagger wiring, middleware
├── HotelBooking.Application     DTOs, service interfaces + implementations, IApplicationDbContext abstraction
├── HotelBooking.Domain          Entities, enums, and pure business logic (no framework dependencies)
└── HotelBooking.Infrastructure  EF Core DbContext, migrations, Identity, JWT issuing, DI wiring

HotelBooking.Tests               Unit tests (Domain logic + services) and integration tests (real HTTP calls)
```

`Domain` has zero dependencies on EF Core or ASP.NET Core — `OverlapChecker` and `PriceCalculator` are plain static methods, which is what makes them trivial to unit test in isolation. `Application` depends only on an `IApplicationDbContext` interface (not the concrete EF Core type), so it stays testable without a real database; `Infrastructure` implements that interface.

## Tech Stack

- **Backend:** C#, ASP.NET Core 8 Web API, EF Core 8, SQLite, ASP.NET Core Identity + JWT bearer auth, Serilog, Swagger/OpenAPI
- **Testing:** xUnit, FluentAssertions, `WebApplicationFactory` for integration tests, SQLite in-memory for transactional unit tests
- **Infrastructure:** Docker

SQLite was chosen over PostgreSQL to keep the project runnable with zero external setup (see [Design Decisions](#design-decisions)).

## Database Schema

| Entity | Key fields |
|---|---|
| `Hotel` | Name, Address, Description |
| `RoomType` | Name, MaxGuests, BasePrice |
| `Room` | HotelId, RoomTypeId, RoomNumber, Floor, **Status** (`Available` / `Maintenance` / `Inactive`) |
| `Guest` | UserId (→ Identity user), FirstName, LastName, Phone |
| `Reservation` | GuestId, RoomId, CheckIn, CheckOut, GuestsCount, **Status** (`Pending` / `Confirmed` / `Cancelled` / `Completed`), TotalPrice |
| `Payment` | ReservationId, Amount, **Status** (`Pending` / `Paid` / `Failed` / `Refunded`), TransactionReference |

Room occupancy is **not** a stored status — see Design Decisions.

## API Documentation

Swagger UI is available at `/swagger` in every environment (including the Docker image).

| Area | Endpoints |
|---|---|
| Auth | `POST /api/auth/register`, `POST /api/auth/login` |
| Rooms | `GET /api/rooms`, `GET /api/rooms/{id}`, `GET /api/rooms/availability?checkIn&checkOut&guests`, `POST/PUT/DELETE /api/rooms` (Admin) |
| Reservations | `POST /api/reservations`, `GET /api/reservations`, `GET /api/reservations/{id}`, `POST /api/reservations/{id}/cancel` |
| Admin | `GET /api/admin/reservations`, `GET /api/admin/dashboard` (both Admin-only) |

Errors follow a consistent shape:

```json
{ "status": 409, "code": "ROOM_NOT_AVAILABLE", "message": "Room 201 is no longer available for the selected dates." }
```

`400` validation, `401` unauthenticated, `403` wrong role, `404` not found, `409` booking conflict, `500` unexpected.

## Authentication

ASP.NET Core Identity issues JWT bearer tokens. Every new registration gets the `Guest` role; an `Admin` account is seeded on first run (`admin@hotelbooking.local` / `Admin#12345` — dev-only credential, not for production use). Room mutations and the `/api/admin/*` endpoints require `[Authorize(Roles = "Admin")]`; reservation endpoints check both authentication and that the caller owns the reservation (or is an Admin) before returning it.

## Booking Flow

```
Search availability → select room → create reservation
  → validate dates (checkout after checkin, not in the past)
  → validate guest count against room capacity
  → check room status
  → BEGIN TRANSACTION
  → re-check availability (search result may be stale)
  → calculate price → create reservation → create payment
  → COMMIT
```

The second availability check, inside the transaction, is the whole point: the client's search result is a snapshot that can go stale between search and booking, so it is never trusted as a lock. See [Concurrency Handling](#concurrency-handling).

## Concurrency Handling

Two reservations overlap when `existing.CheckIn < requested.CheckOut && existing.CheckOut > requested.CheckIn`. This is implemented once, as a pure static method (`OverlapChecker.IsOverlapping`), and reused both in the room-availability search query and in the availability re-check at booking time — so the two checks can't drift apart.

Booking creation wraps the re-check + reservation insert + payment insert in a single database transaction. This was verified with an integration test that fires two concurrent `POST /api/reservations` requests for the same room and overlapping dates: exactly one gets `201 Created`, the other gets `409 Conflict` with `ROOM_NOT_AVAILABLE`. The test was re-run repeatedly during development to confirm it isn't flaky.

## Testing

22 tests, all passing:

- **Unit** — `OverlapChecker` (overlapping/adjacent/disjoint ranges), `PriceCalculator` (subtotal/tax/rounding/invalid dates), and `ReservationService` against a real SQLite in-memory database (needed for transaction support, which EF Core's InMemory provider doesn't have) covering date validation, capacity validation, overlap rejection, adjacent-booking acceptance, and cancellation reopening availability.
- **Integration** — full HTTP flow through `WebApplicationFactory` against an isolated per-test-run SQLite file: booking creates a reservation + payment reflected in the admin dashboard, overlapping bookings return 409, the concurrency race described above, cancellation reopening a room, and unauthenticated requests returning 401.

```bash
dotnet test
```

## Running Locally

Requires the .NET 8 SDK.

```bash
dotnet restore
dotnet run --project src/HotelBooking.Api
```

The API applies EF Core migrations and seeds sample data (one hotel, four room types, seven rooms, the `Admin`/`Guest` roles, and the seed admin account) automatically on startup. Swagger UI is at `http://localhost:<port>/swagger`.

## Docker

```bash
docker compose up --build
```

Serves the API at `http://localhost:8080` (Swagger at `/swagger`), with the SQLite database and log files persisted in named Docker volumes across restarts.

## Design Decisions

- **Room occupancy is derived from reservations, not stored as a permanent room state.** `Room.Status` only tracks `Available` / `Maintenance` / `Inactive` — administrative states. Whether a room is "occupied" on a given date is computed from `Reservation` rows, which avoids a second source of truth that could drift out of sync with the actual bookings.
- **Availability is checked twice: at search and again at booking creation, inside a transaction.** A search result is a snapshot; by the time a guest submits a booking, another guest may have taken the room. Re-validating inside the transaction — rather than trusting the earlier search — is what actually prevents double-booking, and it's covered by a real concurrent-request test rather than just sequential unit tests.
- **SQLite instead of PostgreSQL.** This keeps the project runnable with zero external setup (`dotnet run` and it works), which matters more for a portfolio demo than production-grade concurrent-write throughput. The `Application`/`Infrastructure` split means swapping to PostgreSQL is a matter of changing the EF Core provider and connection string, not rewriting business logic.
- **Admin room management reuses `/api/rooms` with role-based authorization**, instead of duplicating CRUD under a separate `/api/admin/rooms`. One implementation, gated by `[Authorize(Roles = "Admin")]` on the mutating verbs, rather than two copies of the same logic.
- **No HTTPS redirection.** This demo has no TLS termination configured (and the Docker image only exposes plain HTTP); in a real deployment, TLS would sit at a reverse proxy/load balancer in front of the API rather than in the app itself.
