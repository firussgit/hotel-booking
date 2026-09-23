namespace HotelBooking.Domain.Services;

public static class PriceCalculator
{
    public const decimal TaxRate = 0.10m;

    public static PriceBreakdown Calculate(decimal pricePerNight, DateOnly checkIn, DateOnly checkOut)
    {
        var nights = checkOut.DayNumber - checkIn.DayNumber;
        if (nights <= 0)
        {
            throw new ArgumentException("Check-out date must be after check-in date.");
        }

        var subtotal = pricePerNight * nights;
        var tax = decimal.Round(subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);
        var total = subtotal + tax;

        return new PriceBreakdown(nights, subtotal, tax, total);
    }
}
