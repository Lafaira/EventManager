

using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces
{
    public interface IBookingQueue
    {
        void Enqueue(Booking task);
        bool TryDequeue(out Booking task);
    }
}
