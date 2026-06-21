using EventManager.Models;
using EventManager.Services.Interfaces;
using System.Collections.Concurrent;

namespace EventManager.Services
{
    public class BookingQueue : IBookingQueue
    {
        private readonly ConcurrentQueue<Booking> _queue = new();

        public void Enqueue(Booking task)
        {
            _queue.Enqueue(task);
        }

        public bool TryDequeue(out Booking task)
        {
            return _queue.TryDequeue(out task);
        }
    }
}
