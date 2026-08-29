

using BookingService.Booking.Application.Interfaces;
using BookingService.Booking.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Booking.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        AppDbContext _context;
        public BookingRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task AddBookingAsync(Domain.Models.Booking item, CancellationToken ct = default) => await _context.Bookings.AddAsync(item, ct);
        public async Task SaveChangesAsync(CancellationToken ct = default) => await _context.SaveChangesAsync(ct);
        public async Task<bool> IsBookingExist(Guid bookingId, CancellationToken ct = default) => await _context.Bookings.AnyAsync(x => x.Id == bookingId, ct);
        public async Task<Domain.Models.Booking> GetBooking(Guid bookingId, CancellationToken ct = default) => await _context.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, ct);
        public IEnumerable<Domain.Models.Booking> GetPending() => _context.Bookings.Where(x => x.Status == Domain.Models.BookingStatus.Pending);
        public async Task<int> GetBookingCount(Guid userId) => await _context.Bookings.Where(x => x.UserId == userId && (x.Status== Domain.Models.BookingStatus.Confirmed || x.Status == Domain.Models.BookingStatus.Pending)).CountAsync();

    }
}
