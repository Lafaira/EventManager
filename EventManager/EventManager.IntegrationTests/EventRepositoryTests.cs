using EventManager.DataAccess;
using EventManager.Models;
using EventManager.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;

namespace EventManager.IntegrationTests
{
    public class EventRepositoryTests : IAsyncLifetime
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
            context.Database.EnsureCreated();
            return context;
        }
        private async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE bookings, events RESTART IDENTITY CASCADE");
        }

        [Fact]
        public async Task GetAllEventAsync()
        {
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var eventItem1 = new Event()
            {
                Id = 1,
                Title = "Event1",
                StartAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            };

            var eventItem2 = new Event()
            {
                Id = 2,
                Title = "Event2",
                StartAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            };

            context.Events.AddRange(eventItem1, eventItem2);
            await context.SaveChangesAsync();

            var repositorie = new EventRepositorie(context);

            var items = await repositorie.GetAllEventAsync();

            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task GetEventAsync()
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

            context.Add(eventItem);
            await context.SaveChangesAsync();

            var repositorie = new EventRepositorie(context);

            var item = await repositorie.GetEventAsync(eventItem.Id);

            Assert.NotNull(item);
            Assert.Equal(item.Id, eventItem.Id);
        }

        [Fact]
        public async Task AddEventAsync()
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

            var repository = new EventRepositorie(context);
            await repository.AddEventAsync(eventItem);

            await repository.SaveChangesAsync();

            await using var verifyContext = CreateContext();
            var saved = await verifyContext.Events
                .FirstOrDefaultAsync(b => b.Id == eventItem.Id);

            Assert.NotNull(saved);
            Assert.Equal(saved.Id, eventItem.Id);


        }

        [Fact]
        public async Task RemoveEventAsync()
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

            context.Add(eventItem);
            await context.SaveChangesAsync();

            var repositorie = new EventRepositorie(context);

            repositorie.Remove(eventItem);

            await repositorie.SaveChangesAsync();

            await using var verifyContext = CreateContext();
            var saved = await verifyContext.Events
                .FirstOrDefaultAsync(b => b.Id == eventItem.Id);

            Assert.Null(saved);
        }

        [Fact]
        public async Task CheckAvailabilityAsync()
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

            context.Add(eventItem);
            await context.SaveChangesAsync();

            var repositorie = new EventRepositorie(context);

            var isCheak = await repositorie.CheckAvailabilityAsync(eventItem.Id);

            Assert.True(isCheak);
        }

    }
}
