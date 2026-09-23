namespace HotelBooking.Domain.Entities;

public class Guest
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
