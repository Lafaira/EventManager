using EventManager.DataAccess;
using EventManager.Models;
using EventManager.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Testcontainers.PostgreSql;

namespace EventManager.IntegrationTests
{
    public class BookingRepositoryTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        public async Task InitializeAsync() => await _postgres.StartAsync();
        public async Task DisposeAsync() => await _postgres.DisposeAsync();

        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql(_postgres.GetConnectionString())
        .Options;

            var context = new AppDbContext(options);
            context.Database.Migrate();
            return context;
        }
        private async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE bookings, events RESTART IDENTITY CASCADE");
        }

        [Fact]
        public async Task AddBookingAsync_SavesBookingToDatabase()
        {
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var eventItem = new Event()
            {
                Id = 1,
                Title = "Event1",
                StartAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            };

            context.Events.Add(eventItem);
            await context.SaveChangesAsync();

            var repository = new BookingRepository(context);

            var booking = new Booking(eventItem.Id, BookingStatus.Pending);

            await repository.AddBookingAsync(booking);
            await repository.SaveChangesAsync();

            await using var verifyContext = CreateContext();
            var saved = await verifyContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == booking.Id);

            Assert.NotNull(saved);
            Assert.Equal(BookingStatus.Pending, saved.Status);


        }

        [Fact]
        public async Task GetBooking()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();

            var eventItem = new Event()
            {
                Id = 1,
                Title = "Event1",
                StartAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            };

            context.Events.Add(eventItem);
            await context.SaveChangesAsync();

            var booking = new Booking(eventItem.Id, BookingStatus.Pending);

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();


            var repository = new BookingRepository(CreateContext());
            var booking2 = await repository.GetBooking(booking.Id);

            Assert.Equal(booking.Id, booking2.Id);
            Assert.Equal(eventItem.Id, booking2.EventId);
        }

        [Fact]
        public async Task IsBookingExist()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();

            var eventItem = new Event()
            {
                Id = 1,
                Title = "Event1",
                StartAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            };

            context.Events.Add(eventItem);
            await context.SaveChangesAsync();

            var booking = new Booking(eventItem.Id, BookingStatus.Pending);

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            var repository = new BookingRepository(CreateContext());
            var isExist = await repository.IsBookingExist(booking.Id);

            Assert.True(isExist);
        }

        [Fact]
        public async Task GetPending()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();

            var eventItem = new Event()
            {
                Id = 1,
                Title = "Event1",
                StartAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            };

            context.Events.Add(eventItem);
            await context.SaveChangesAsync();

            var booking1 = new Booking(eventItem.Id, BookingStatus.Pending);
            var booking2 = new Booking(eventItem.Id, BookingStatus.Pending);

            context.Bookings.AddRange(booking1, booking2);
            await context.SaveChangesAsync();

            var repository = new BookingRepository(CreateContext());
            var pending = repository.GetPending();

            Assert.Equal(2, pending.Count());
        }

        [Fact]
        public async Task TablesAndFK()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();

            var conn = context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            using var checkTables = conn.CreateCommand();
            checkTables.CommandText = @"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name IN ('bookings', 'events');";

            var tableCount = Convert.ToInt32(await checkTables.ExecuteScalarAsync());
            Assert.Equal(2, tableCount); 

            using var checkFk = conn.CreateCommand();
            checkFk.CommandText = @"
            SELECT COUNT(*) 
            FROM information_schema.table_constraints 
            WHERE table_schema = 'public' 
            AND table_name = 'bookings' 
            AND constraint_type = 'FOREIGN KEY';";

            var fkCount = Convert.ToInt32(await checkFk.ExecuteScalarAsync());
            Assert.Equal(1, fkCount);
        }

    }
    }
