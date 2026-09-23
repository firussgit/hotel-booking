using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using HotelBooking.Application.Dtos.Admin;
using HotelBooking.Application.Dtos.Reservations;
using HotelBooking.Application.Dtos.Rooms;
using HotelBooking.Tests.Support;

namespace HotelBooking.Tests.Integration;

public class ReservationsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ReservationsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, int roomId)> CreateAuthenticatedGuestClientAsync(string emailPrefix)
    {
        var client = _factory.CreateClient();
        var token = await TestAuthHelper.RegisterAndLoginGuestAsync(client, $"{emailPrefix}-{Guid.NewGuid():N}@test.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var rooms = await client.GetFromJsonAsync<List<RoomDto>>("/api/rooms");
        var roomId = rooms!.First(r => r.Status == "Available").Id;

        return (client, roomId);
    }

    [Fact]
    public async Task CreateReservation_ReturnsConfirmedReservation_AndCreatesPayment()
    {
        var (client, roomId) = await CreateAuthenticatedGuestClientAsync("booker");

        var response = await client.PostAsJsonAsync("/api/reservations", new CreateReservationDto(
            roomId, new DateOnly(2027, 1, 10), new DateOnly(2027, 1, 13), 1));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var reservation = await response.Content.ReadFromJsonAsync<ReservationDto>();
        reservation!.Status.Should().Be("Confirmed");
        reservation.TotalPrice.Should().BeGreaterThan(0);

        var adminClient = _factory.CreateClient();
        var adminToken = await TestAuthHelper.LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var dashboard = await adminClient.GetFromJsonAsync<DashboardDto>("/api/admin/dashboard");
        dashboard!.TotalRevenue.Should().BeGreaterThanOrEqualTo(reservation.TotalPrice);
    }

    [Fact]
    public async Task CreateReservation_RejectsOverlappingBooking_With409()
    {
        var (client, roomId) = await CreateAuthenticatedGuestClientAsync("first");
        var dto = new CreateReservationDto(roomId, new DateOnly(2027, 2, 10), new DateOnly(2027, 2, 15), 1);

        var first = await client.PostAsJsonAsync("/api/reservations", dto);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var (otherClient, _) = await CreateAuthenticatedGuestClientAsync("second");
        var overlapping = new CreateReservationDto(roomId, new DateOnly(2027, 2, 12), new DateOnly(2027, 2, 14), 1);
        var second = await otherClient.PostAsJsonAsync("/api/reservations", overlapping);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ConcurrentBookingAttempts_OnSameRoomAndDates_OnlyOneSucceeds()
    {
        var (clientA, roomId) = await CreateAuthenticatedGuestClientAsync("race-a");
        var (clientB, _) = await CreateAuthenticatedGuestClientAsync("race-b");

        var dto = new CreateReservationDto(roomId, new DateOnly(2027, 3, 10), new DateOnly(2027, 3, 13), 1);

        var taskA = clientA.PostAsJsonAsync("/api/reservations", dto);
        var taskB = clientB.PostAsJsonAsync("/api/reservations", dto);
        var results = await Task.WhenAll(taskA, taskB);

        var successCount = results.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = results.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        successCount.Should().Be(1);
        conflictCount.Should().Be(1);
    }

    [Fact]
    public async Task CancelReservation_ThenRebooking_SameDatesSucceeds()
    {
        var (client, roomId) = await CreateAuthenticatedGuestClientAsync("canceller");
        var dto = new CreateReservationDto(roomId, new DateOnly(2027, 4, 10), new DateOnly(2027, 4, 12), 1);

        var created = await client.PostAsJsonAsync("/api/reservations", dto);
        var reservation = await created.Content.ReadFromJsonAsync<ReservationDto>();

        var cancel = await client.PostAsync($"/api/reservations/{reservation!.Id}/cancel", content: null);
        cancel.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var rebooked = await client.PostAsJsonAsync("/api/reservations", dto);
        rebooked.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Reservations_RequireAuthentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/reservations");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
