using FluentAssertions;
using HotelBooking.Domain.Services;

namespace HotelBooking.Tests.Unit;

public class PriceCalculatorTests
{
    [Fact]
    public void CalculateReservationPrice_ComputesSubtotalTaxAndTotal()
    {
        // 3 nights at €100/night: subtotal €300, 10% tax €30, total €330
        var breakdown = PriceCalculator.Calculate(100m, new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 13));

        breakdown.Nights.Should().Be(3);
        breakdown.Subtotal.Should().Be(300m);
        breakdown.Tax.Should().Be(30m);
        breakdown.Total.Should().Be(330m);
    }

    [Fact]
    public void CalculateReservationPrice_RoundsTaxToTwoDecimalPlaces()
    {
        var breakdown = PriceCalculator.Calculate(33.33m, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2));

        breakdown.Nights.Should().Be(1);
        breakdown.Subtotal.Should().Be(33.33m);
        breakdown.Tax.Should().Be(3.33m);
    }

    [Fact]
    public void ShouldRejectInvalidDates_WhenCheckOutEqualsCheckIn()
    {
        var act = () => PriceCalculator.Calculate(100m, new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 10));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ShouldRejectInvalidDates_WhenCheckOutIsBeforeCheckIn()
    {
        var act = () => PriceCalculator.Calculate(100m, new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 5));

        act.Should().Throw<ArgumentException>();
    }
}
