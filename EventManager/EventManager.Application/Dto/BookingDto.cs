using EventManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Dto
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public int EventId { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
