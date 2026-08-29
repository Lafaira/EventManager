using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Interfaces
{
    public interface IKafkaProduser
    {
        public Task CreateProducer(int eventId, BookingService.Booking.Domain.Models.Booking booking);
    }
}
