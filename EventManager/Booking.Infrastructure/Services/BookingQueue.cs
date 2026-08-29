using BookingService.Booking.Application.Interfaces;
using System.Collections.Concurrent;

namespace BookingService.Booking.Infrastructure.Services
{
    public class BookingQueue : IBookingQueue
    {
        private readonly ConcurrentQueue<Domain.Models.Booking> _queue = new();

        public void Enqueue(Domain.Models.Booking task)
        {
            _queue.Enqueue(task);
        }

        public bool TryDequeue(out Domain.Models.Booking task)
        {
            return _queue.TryDequeue(out task);
        }
    }
}
