namespace HotelBooking.Domain.Services;

public record PriceBreakdown(int Nights, decimal Subtotal, decimal Tax, decimal Total);
