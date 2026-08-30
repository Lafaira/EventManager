
namespace BookingService.Booking.Application.Interfaces
{
    public interface IBookingRepository
    {
        public Task AddBookingAsync(Domain.Models.Booking item, CancellationToken ct = default);
        public Task SaveChangesAsync(CancellationToken ct = default);
        public Task<bool> IsBookingExist(Guid bookingId, CancellationToken ct = default);
        public Task<Domain.Models.Booking> GetBooking(Guid bookingId, CancellationToken ct = default);
        public IEnumerable<Domain.Models.Booking> GetPending();
        public Task<int> GetBookingCount(Guid userId);
    }
}
