using Event.Application.Interfaces;
using Event.Infrastructure.Services;
using EventService.Event.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace EventService.Test
{
    public class EventServiceTest
    {
        private readonly Mock<IConnectionMultiplexer> _mockMultiplexer;
        private readonly Mock<IDatabase> _mockDatabase;
        private readonly Mock<IEventRepository> _mockRepository;
        private readonly Mock<ILogger<RedisService>> _mockLogger;
        private readonly Mock<ICache> _mockCache;
        private readonly RedisService _redis;
        private readonly EventService.Event.Application.Services.EventService _eventService;

        public EventServiceTest()
        {
            _mockMultiplexer = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();
            _mockRepository = new Mock<IEventRepository>();
            _mockLogger = new Mock<ILogger<RedisService>>();
            _mockCache = new Mock<ICache>();

            _mockMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_mockDatabase.Object);


            _redis = new RedisService(_mockMultiplexer.Object, _mockRepository.Object, _mockLogger.Object);
            _eventService = new EventService.Event.Application.Services.EventService(_mockRepository.Object, _mockCache.Object);

        }

        [Fact]
        public async Task GetByIdAsync_CacheHitNotCallRepository()
        {
            int eventId = 1;
            var eventItem = new EventService.Event.Domain.Models.Event
            { 
                Id = eventId,
                Title = "Test1"
            };

            string cachedJson = JsonSerializer.Serialize(eventItem);

            _mockDatabase.Setup(db => db.StringGetAsync($"event:{eventId}", It.IsAny<CommandFlags>()))
                .ReturnsAsync((RedisValue)cachedJson);

            var result = await _redis.GetByIdAsync(eventId);

            Assert.NotNull(result);
            Assert.Equal(eventItem.Title, result.Title);
            _mockRepository.Verify(repo => repo.GetEventAsync(eventId, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetTop10Async_CacheHitNotCallRepository()
        {
            int eventId = 1;
            var eventItem = new List<EventService.Event.Domain.Models.Event>()
            {
                new EventService.Event.Domain.Models.Event()
                {
                    Id = eventId,
                    Title = "Test1"
                }
            };

            string cachedJson = JsonSerializer.Serialize(eventItem);

            _mockDatabase.Setup(db => db.StringGetAsync($"events:top10", It.IsAny<CommandFlags>()))
                .ReturnsAsync((RedisValue)cachedJson);

            var result = await _redis.GetTop10();

            Assert.NotNull(result);
            Assert.Equal(eventItem.First().Title, result.First().Title);
            _mockRepository.Verify(repo => repo.GetEventAsync(eventId, It.IsAny<CancellationToken>()), Times.Never);
        }


        [Fact]
        public async Task GetByIdAsync_FetchesFromRepositoryAndSavesToCache()
        {
            int eventId = 1;
            var eventItem = new EventService.Event.Domain.Models.Event
            {
                Id = eventId,
                Title = "Test1"
            };

            _mockDatabase.Setup(db => db.StringGetAsync($"event:{eventId}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            _mockRepository.Setup(repo => repo.GetEventAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventItem);

            var result = await _redis.GetByIdAsync(eventId);

            Assert.NotNull(result);
            Assert.Equal(eventItem.Title, result.Title);
            _mockRepository.Verify(repo => repo.GetEventAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);

        }


        [Fact]
        public async Task CheckTryReserveSeatsAsync_Success_InvalidatesCache()
        {
            int eventId = 1;

            var eventItem = new EventService.Event.Domain.Models.Event
            {
                Id = eventId,
                TotalSeats = 10,
                AvailableSeats = 5
            };

            _mockRepository.Setup(r => r.GetEventAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(eventItem);
            _mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _eventService.CheckTryReserveSeatsAsync(eventId);

            Assert.True(result);

            _mockCache.Verify(c => c.RemoveCacheEventById(eventId), Times.Once);
        }

    }
}
