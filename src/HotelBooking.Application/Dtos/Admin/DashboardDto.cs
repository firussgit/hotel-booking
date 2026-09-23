namespace HotelBooking.Application.Dtos.Admin;

public record DashboardDto(int TotalRooms, double OccupancyRatePercent, decimal TotalRevenue);
