using Event.Application.Interfaces;
using EventService.Event.Application.Interfaces;
using EventService.Event.Domain.Models;
using EventService.Event.Infrastructure.DataAccess;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text.Json;
using static Confluent.Kafka.ConfigPropertyNames;

namespace Event.Infrastructure.Services
{
    public class RedisService : ICache
    {
        private readonly IDatabase _redis;
        private readonly IEventRepository _db;
        private readonly ILogger<RedisService> _logger;
        public RedisService(IConnectionMultiplexer connection, IEventRepository db, ILogger<RedisService> logger) 
        {
            _redis = connection.GetDatabase();
            _db = db;
            _logger = logger;
        }

        public async Task<EventService.Event.Domain.Models.Event?> GetByIdAsync(int id)
        {
           
            var cacheKey = $"event:{id}";

            var cached = await _redis.StringGetAsync(cacheKey);
            if (cached.HasValue)
            {
                return System.Text.Json.JsonSerializer.Deserialize<EventService.Event.Domain.Models.Event>((string)cached!);
            }

            var eventItem = await _db.GetEventAsync(id);
            if (eventItem == null) return null;

            await _redis.StringSetAsync(
            cacheKey,
            JsonSerializer.Serialize(eventItem),
            TimeSpan.FromMinutes(5)
        );

            return eventItem;
        }

       public async Task<List<EventService.Event.Domain.Models.Event?>> GetTop10()
       {
            var cacheKey = "events:top10";

            var cached = await _redis.StringGetAsync(cacheKey);

            if (cached.HasValue)
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<EventService.Event.Domain.Models.Event>?>((string)cached!);
            }

            var events = await _db.GetAllEventAsync();

            var top10 = events.OrderByDescending(x => (double)(x.TotalSeats - x.AvailableSeats) / x.TotalSeats).Take(10).ToList();

            if (top10 == null) return null;

            await _redis.StringSetAsync(
                cacheKey,
                JsonSerializer.Serialize(top10),
                TimeSpan.FromMinutes(5)
                );

            return top10;
        }

        public async Task RemoveCacheEventById(int id)
        {
            string cacheKey = $"event:{id}";
            await _redis.KeyDeleteAsync(cacheKey);
        }
    }
}
