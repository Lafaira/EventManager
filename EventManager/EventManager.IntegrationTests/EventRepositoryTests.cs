using EventManager.DataAccess;
using EventManager.Models;
using EventManager.Models.RequestModel;
using EventManager.Repositories;
using EventManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            //context.Database.EnsureDeleted();
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

            var repositorie = new EventRepository(context);

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

            var repositorie = new EventRepository(context);

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

            var repository = new EventRepository(context);
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

            var repositorie = new EventRepository(context);

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

            var repositorie = new EventRepository(context);

            var isCheak = await repositorie.CheckAvailabilityAsync(eventItem.Id);

            Assert.True(isCheak);
        }

        List<Event> _events = new List<Event>()
        {
            new Event()
            {
                Id = 5,
                Title = "Event",
                StartAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            },
            new Event()
            {
                Id = 1,
                Title = "Event1",
                StartAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            },
            new Event()
            {
                Id = 2,
                Title = "Event2",
                StartAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            },
            new Event()
            {
                Id = 3,
                Title = "Event3",
                StartAt = new DateTime(2023, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            },
            new Event()
            {
                Id = 4,
                Title = "Test",
                StartAt = new DateTime(2023, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            },

        };


        [Fact]
        public async Task GetAllEvents_WithTitleFilter()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();

            context.Events.AddRange(_events);
            await context.SaveChangesAsync();

            var repository = new EventRepository(context);
            var service = new EventService(repository);
            var pageInfo = new PageInfo { Page = 1, PageSize = 10 };
            var filter = new GetEventsQuery { Title = "Event" }; 

            var result = await service.GetAllEventsAsync(pageInfo, filter);

            Assert.Equal(4, result.CountEvent); 
            
        }

        [Fact]
        public async Task GetAllEvents_WithDataFilter()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();

            context.Events.AddRange(_events);
            await context.SaveChangesAsync();

            var repository = new EventRepository(context);
            var service = new EventService(repository);
            var pageInfo = new PageInfo { Page = 1, PageSize = 10 };
            var filter = new GetEventsQuery { To = new DateTime(2024, 09, 01, 0, 0, 0, DateTimeKind.Utc), From = new DateTime(2022, 01, 01, 0, 0, 0, DateTimeKind.Utc) };

            var result = await service.GetAllEventsAsync(pageInfo, filter);

            Assert.Equal(2, result.CountEvent);

        }

        [Fact]
        public async Task GetAllEvents_Pagination()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();

            context.Events.AddRange(_events);
            await context.SaveChangesAsync();

            var repository = new EventRepository(context);
            var service = new EventService(repository);
            var pageInfo = new PageInfo { Page = 1, PageSize = 2 };
           

            var result = await service.GetAllEventsAsync(pageInfo);

            Assert.Equal(2, result.EventArr.Count());

        }

    }
}
