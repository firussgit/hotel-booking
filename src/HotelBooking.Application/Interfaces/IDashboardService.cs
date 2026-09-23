using HotelBooking.Application.Dtos.Admin;

namespace HotelBooking.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
