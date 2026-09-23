namespace HotelBooking.Domain.Services;

public static class OverlapChecker
{
    public static bool IsOverlapping(DateOnly existingCheckIn, DateOnly existingCheckOut, DateOnly requestedCheckIn, DateOnly requestedCheckOut)
        => existingCheckIn < requestedCheckOut && existingCheckOut > requestedCheckIn;
}
