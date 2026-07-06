using EventManager.Models;

namespace EventManager.Services.Interfaces
{
    public interface IBookingService
    {
        public Task<Booking> CreateBookingAsync(int eventId, CancellationToken ct = default);
        public Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default);
        public IEnumerable<Booking> GetPending();
    }
}
