
using Contracts;

namespace BookingService.Booking.Application.ApplicationInterfaces
{
    public interface IBookingService
    {
        public Task<Domain.Models.Booking> CreateBookingAsync(int eventId, Guid userId, CancellationToken ct = default);
        public Task<Domain.Models.Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default);
        public IEnumerable<Domain.Models.Booking> GetPending();
        public Task UpdateBooking(Domain.Models.Booking booking, CancellationToken ct = default);
        public Task<bool> CancelledBooking(Guid id, Guid userId, RolesEnum role);
    }
}
