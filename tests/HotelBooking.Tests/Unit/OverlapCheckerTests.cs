using FluentAssertions;
using HotelBooking.Domain.Services;

namespace HotelBooking.Tests.Unit;

public class OverlapCheckerTests
{
    private static DateOnly D(int day) => new(2026, 10, day);

    [Fact]
    public void ShouldDetectOverlappingReservation_WhenRequestedRangeIsInsideExisting()
    {
        // Existing: Oct 10 -> Oct 15, Requested: Oct 12 -> Oct 14
        var overlapping = OverlapChecker.IsOverlapping(D(10), D(15), D(12), D(14));

        overlapping.Should().BeTrue();
    }

    [Theory]
    [InlineData(14, 18)] // requested starts before existing ends
    [InlineData(9, 11)]  // requested ends after existing starts
    [InlineData(9, 16)]  // requested fully contains existing
    public void ShouldDetectOverlappingReservation_ForPartialOverlaps(int requestedStartDay, int requestedEndDay)
    {
        var overlapping = OverlapChecker.IsOverlapping(D(10), D(15), D(requestedStartDay), D(requestedEndDay));

        overlapping.Should().BeTrue();
    }

    [Fact]
    public void ShouldAllowAdjacentReservations_WhenRequestedStartsOnExistingCheckOut()
    {
        // Existing: Oct 10 -> Oct 15, Requested: Oct 15 -> Oct 18
        var overlapping = OverlapChecker.IsOverlapping(D(10), D(15), D(15), D(18));

        overlapping.Should().BeFalse();
    }

    [Fact]
    public void ShouldAllowAdjacentReservations_WhenRequestedEndsOnExistingCheckIn()
    {
        // Existing: Oct 10 -> Oct 15, Requested: Oct 7 -> Oct 10
        var overlapping = OverlapChecker.IsOverlapping(D(10), D(15), D(7), D(10));

        overlapping.Should().BeFalse();
    }

    [Fact]
    public void ShouldNotOverlap_WhenCompletelySeparate()
    {
        var overlapping = OverlapChecker.IsOverlapping(D(10), D(15), D(20), D(25));

        overlapping.Should().BeFalse();
    }
}
