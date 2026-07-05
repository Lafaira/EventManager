using EventManager.Models;

namespace EventManager.Services.Interfaces
{
    public interface IBookingService
    {
        public Booking CreateBookingAsync(int eventId);
        public Booking GetBookingByIdAsync(Guid bookingId);
        public IEnumerable<Booking> GetPending();
    }
}
