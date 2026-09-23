using FluentAssertions;
using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Dtos.Reservations;
using HotelBooking.Application.Services;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using HotelBooking.Infrastructure.Persistence;
using HotelBooking.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;

namespace HotelBooking.Tests.Unit;

public class ReservationServiceTests : IDisposable
{
    private readonly SqliteInMemoryContextFactory _factory = new();
    private const string GuestUserId = "guest-user-1";

    public ReservationServiceTests()
    {
        using var context = _factory.CreateContext();
        Seed(context);
    }

    private static void Seed(HotelBookingDbContext context)
    {
        var hotel = new Hotel { Name = "Test Hotel", Address = "1 Test St" };
        var roomType = new RoomType { Name = "Double", MaxGuests = 2, BasePrice = 100m };
        var room = new Room { Hotel = hotel, RoomType = roomType, RoomNumber = "101", Floor = 1, Status = RoomStatus.Available };

        context.Hotels.Add(hotel);
        context.RoomTypes.Add(roomType);
        context.Rooms.Add(room);
        context.Guests.Add(new Guest { UserId = GuestUserId, FirstName = "Jane", LastName = "Doe" });
        context.SaveChanges();
    }

    private ReservationService CreateSut(HotelBookingDbContext context)
        => new(context, new AvailabilityService(context), NullLogger<ReservationService>.Instance);

    [Fact]
    public async Task ShouldRejectInvalidDates_WhenCheckOutIsNotAfterCheckIn()
    {
        using var context = _factory.CreateContext();
        var sut = CreateSut(context);
        var room = context.Rooms.Single();
        var dto = new CreateReservationDto(room.Id, new DateOnly(2026, 10, 15), new DateOnly(2026, 10, 15), 1);

        var act = () => sut.CreateAsync(GuestUserId, dto);

        await act.Should().ThrowAsync<ValidationAppException>();
    }

    [Fact]
    public async Task ShouldRejectMoreGuestsThanRoomCapacity()
    {
        using var context = _factory.CreateContext();
        var sut = CreateSut(context);
        var room = context.Rooms.Single(); // MaxGuests = 2
        var dto = new CreateReservationDto(room.Id, new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 12), 3);

        var act = () => sut.CreateAsync(GuestUserId, dto);

        await act.Should().ThrowAsync<ValidationAppException>();
    }

    [Fact]
    public async Task CreateAsync_ComputesTotalPriceAndConfirmsReservation_ForValidRequest()
    {
        using var context = _factory.CreateContext();
        var sut = CreateSut(context);
        var room = context.Rooms.Single();
        var dto = new CreateReservationDto(room.Id, new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 13), 2);

        var result = await sut.CreateAsync(GuestUserId, dto);

        result.Status.Should().Be(nameof(ReservationStatus.Confirmed));
        result.TotalPrice.Should().Be(330m); // 3 nights * 100 + 10% tax
    }

    [Fact]
    public async Task CreateAsync_RejectsOverlappingReservation_WithConflict()
    {
        using var context = _factory.CreateContext();
        var sut = CreateSut(context);
        var room = context.Rooms.Single();

        await sut.CreateAsync(GuestUserId, new CreateReservationDto(room.Id, new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 15), 1));

        var act = () => sut.CreateAsync(GuestUserId, new CreateReservationDto(room.Id, new DateOnly(2026, 10, 12), new DateOnly(2026, 10, 14), 1));

        await act.Should().ThrowAsync<ConflictException>().Where(e => e.Code == "ROOM_NOT_AVAILABLE");
    }

    [Fact]
    public async Task CreateAsync_AllowsAdjacentReservation_ImmediatelyAfterExistingCheckOut()
    {
        using var context = _factory.CreateContext();
        var sut = CreateSut(context);
        var room = context.Rooms.Single();

        await sut.CreateAsync(GuestUserId, new CreateReservationDto(room.Id, new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 15), 1));

        var act = () => sut.CreateAsync(GuestUserId, new CreateReservationDto(room.Id, new DateOnly(2026, 10, 15), new DateOnly(2026, 10, 18), 1));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CancelAsync_ReopensAvailability_ForTheCancelledDateRange()
    {
        using var context = _factory.CreateContext();
        var sut = CreateSut(context);
        var room = context.Rooms.Single();
        var dto = new CreateReservationDto(room.Id, new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 15), 1);

        var reservation = await sut.CreateAsync(GuestUserId, dto);
        await sut.CancelAsync(GuestUserId, isAdmin: false, reservation.Id);

        var act = () => sut.CreateAsync(GuestUserId, dto);

        await act.Should().NotThrowAsync();
    }

    public void Dispose() => _factory.Dispose();
}
