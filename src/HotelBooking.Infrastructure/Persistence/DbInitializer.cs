using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(HotelBookingDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Hotels.AnyAsync())
        {
            return;
        }

        var hotel = new Hotel
        {
            Name = "Grand Azure Hotel",
            Address = "1 Harbor View Rd, Coastal City",
            Description = "A seaside hotel with modern rooms and full amenities."
        };

        var single = new RoomType { Name = "Single", Description = "Cozy room for one guest.", MaxGuests = 1, BasePrice = 60m };
        var doubleRoom = new RoomType { Name = "Double", Description = "Comfortable room for two guests.", MaxGuests = 2, BasePrice = 90m };
        var deluxe = new RoomType { Name = "Deluxe", Description = "Spacious room with premium furnishings.", MaxGuests = 3, BasePrice = 120m };
        var suite = new RoomType { Name = "Suite", Description = "Top-tier suite with separate living area.", MaxGuests = 4, BasePrice = 200m };

        hotel.Rooms.Add(new Room { RoomNumber = "101", Floor = 1, RoomType = single, Status = RoomStatus.Available });
        hotel.Rooms.Add(new Room { RoomNumber = "102", Floor = 1, RoomType = single, Status = RoomStatus.Available });
        hotel.Rooms.Add(new Room { RoomNumber = "201", Floor = 2, RoomType = doubleRoom, Status = RoomStatus.Available });
        hotel.Rooms.Add(new Room { RoomNumber = "202", Floor = 2, RoomType = doubleRoom, Status = RoomStatus.Available });
        hotel.Rooms.Add(new Room { RoomNumber = "204", Floor = 2, RoomType = deluxe, Status = RoomStatus.Available });
        hotel.Rooms.Add(new Room { RoomNumber = "301", Floor = 3, RoomType = suite, Status = RoomStatus.Available });
        hotel.Rooms.Add(new Room { RoomNumber = "302", Floor = 3, RoomType = suite, Status = RoomStatus.Maintenance });

        context.Hotels.Add(hotel);
        context.RoomTypes.AddRange(single, doubleRoom, deluxe, suite);

        await context.SaveChangesAsync();
    }
}
