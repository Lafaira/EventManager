using EventManager.Models;

namespace EventManager.Services.Interfaces
{
    public interface IBookingQueue
    {
        void Enqueue(Booking task);
        bool TryDequeue(out Booking task);
    }
}
