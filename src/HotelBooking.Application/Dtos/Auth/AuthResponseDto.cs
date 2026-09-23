namespace HotelBooking.Application.Dtos.Auth;

public record AuthResponseDto(string Token, DateTime ExpiresAtUtc, string Email, IEnumerable<string> Roles);
