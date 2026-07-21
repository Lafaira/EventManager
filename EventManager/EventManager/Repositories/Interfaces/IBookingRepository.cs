using EventManager.Models;

namespace EventManager.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        public Task AddBookingAsync(Booking item, CancellationToken ct = default);
        public Task SaveChangesAsync(CancellationToken ct = default);
        public Task<bool> IsBookingExist(Guid bookingId, CancellationToken ct = default);
        public Task<Booking> GetBooking(Guid bookingId, CancellationToken ct = default);
        public IEnumerable<Booking> GetPending();
    }
}
