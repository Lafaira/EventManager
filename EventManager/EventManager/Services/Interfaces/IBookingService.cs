using EventManager.Models;

namespace EventManager.Services.Interfaces
{
    public interface IBookingService
    {
        public Booking CreateBooking(int eventId);
        public Booking GetBookingById(Guid bookingId);
        public bool SaveBooking(Booking booking);
    }
}
