using System.Net.Http.Json;
using HotelBooking.Application.Dtos.Auth;

namespace HotelBooking.Tests.Support;

public static class TestAuthHelper
{
    public static async Task<string> RegisterAndLoginGuestAsync(HttpClient client, string email)
    {
        var register = new RegisterDto(email, "Passw0rd!", "Test", "Guest", "555-0000");
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", register);
        registerResponse.EnsureSuccessStatusCode();

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }

    public static async Task<string> LoginAdminAsync(HttpClient client)
    {
        var login = new LoginDto("admin@hotelbooking.local", "Admin#12345");
        var response = await client.PostAsJsonAsync("/api/auth/login", login);
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }
}
