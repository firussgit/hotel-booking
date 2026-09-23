namespace HotelBooking.Application.Dtos.Auth;

public record RegisterDto(string Email, string Password, string FirstName, string LastName, string? Phone);
