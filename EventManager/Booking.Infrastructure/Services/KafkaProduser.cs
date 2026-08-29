using Booking.Application.Interfaces;
using Confluent.Kafka;
using Contracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Booking.Infrastructure.Services
{
    public class KafkaProduser : IKafkaProduser
    {
        IConfiguration _configuration;
        IProducer<string, string> _producer;
        public KafkaProduser(IConfiguration configuration) 
        {
            _configuration = configuration;

            var config = new ProducerConfig
            {
                BootstrapServers = _configuration["BootstrapServers"]
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }
        public async Task CreateProducer(int eventId, BookingService.Booking.Domain.Models.Booking booking)
        {
            var bookingConfirmed = new BookingConfirmed()
            {
                BookingId = booking.Id,
                EventId = eventId,
                ConfirmedAt = booking.CreatedAt,
                SeatsCount = 1,
                UserId = booking.UserId,
            };

            var result = await _producer.ProduceAsync(KafkaTopics.BookingConfirmed, new Message<string, string>
            {
                Key = eventId.ToString(),
                Value = JsonSerializer.Serialize(bookingConfirmed)
            });



        }



    }
}
