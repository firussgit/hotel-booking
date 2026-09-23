using HotelBooking.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Support;

/// <summary>
/// Creates a HotelBookingDbContext backed by a SQLite in-memory database that stays open
/// for the lifetime of the connection, so it supports real transactions (unlike the EF Core
/// InMemory provider) while remaining fast and isolated between tests.
/// </summary>
public sealed class SqliteInMemoryContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteInMemoryContextFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public HotelBookingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelBookingDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new HotelBookingDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
