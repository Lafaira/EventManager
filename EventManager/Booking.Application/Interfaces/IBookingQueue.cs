
namespace BookingService.Booking.Application.Interfaces
{
    public interface IBookingQueue
    {
        void Enqueue(Domain.Models.Booking task);
        bool TryDequeue(out Domain.Models.Booking task);
    }
}
