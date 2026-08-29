using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Contracts
{
    public class BookingConfirmed
    {
        public Guid BookingId { get; init; }

        public int EventId { get; init; }

        public Guid UserId { get; init; }

        public int SeatsCount { get; init; }

        public DateTime ConfirmedAt { get; init; }
    }
}
