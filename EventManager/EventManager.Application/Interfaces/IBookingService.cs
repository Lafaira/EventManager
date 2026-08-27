

using EventManager.Domain.Models;

namespace EventManager.ApplicationInterfaces
{
    public interface IBookingService
    {
        public Task<Booking> CreateBookingAsync(int eventId, Guid userId, CancellationToken ct = default);
        public Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default);
        public IEnumerable<Booking> GetPending();
        public Task UpdateBooking(Booking booking, CancellationToken ct = default);
        public Task<bool> CancelledBooking(Guid id, Guid userId, RolesEnum role);
    }
}
