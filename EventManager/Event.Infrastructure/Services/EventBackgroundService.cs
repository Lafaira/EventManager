using Confluent.Kafka;
using Contracts;
using EventService.Event.Application.Interfaces;
using EventService.Event.Application.Services;
using EventService.Event.DomainModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using static Confluent.Kafka.ConfigPropertyNames;

namespace Event.Infrastructure.Services
{
    public class EventBackgroundService : BackgroundService
    {
        private ILogger<EventBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private IConsumer<string, string> _consumer;
        public EventBackgroundService(ILogger<EventBackgroundService> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _logger = logger;

            _scopeFactory = scopeFactory;

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["BootstrapServers"],
                GroupId = "event-processing",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();

            _consumer.Subscribe(KafkaTopics.BookingConfirmed);

        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("EventBackgroundService запущен");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(ct);

                    var bookingConfirmed = JsonSerializer.Deserialize<BookingConfirmed>(consumeResult.Message.Value);
                   
                    int eventId = bookingConfirmed.EventId; 
                    DateTime bookingCreatedAt = bookingConfirmed.ConfirmedAt;

                    _consumer.StoreOffset(consumeResult);
                    _consumer.Commit();

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

                        var cheeckAvailability = await eventService.CheckAvailabilityAsync(eventId, ct);
                        if (!cheeckAvailability)
                        {
                            _logger.LogError("Событие с таким id не существует");
                            continue;
                        }

                        var checkSeats = await eventService.CheckTryReserveSeatsAsync(eventId, ct); // тут уменьшаю места
                        if (!checkSeats)
                        {
                            _logger.LogError("Закончились места на событие");
                            continue;
                        }

                        var eventItem = await eventService.GetEventAsync(eventId, ct);
                        
                        if (eventItem.StartAt < bookingCreatedAt) 
                        {
                            _logger.LogError("Нельзя забронировать событие, которое уже началось");
                            continue;
                        }
                    }
                   
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, $"Ошибка при получении сообщения: {ex.Error.Reason}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении брони");
                }


                await Task.Delay(TimeSpan.FromSeconds(2), ct);
            }
        }
    }
}
