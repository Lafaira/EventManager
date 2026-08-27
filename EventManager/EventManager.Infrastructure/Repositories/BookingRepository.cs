

using EventManager.Application.Interfaces;
using EventManager.Domain.Models;
using EventManager.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        AppDbContext _context;
        public BookingRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task AddBookingAsync(Booking item, CancellationToken ct = default) => await _context.Bookings.AddAsync(item, ct);
        public async Task SaveChangesAsync(CancellationToken ct = default) => await _context.SaveChangesAsync(ct);
        public async Task<bool> IsBookingExist(Guid bookingId, CancellationToken ct = default) => await _context.Bookings.AnyAsync(x => x.Id == bookingId, ct);
        public async Task<Booking> GetBooking(Guid bookingId, CancellationToken ct = default) => await _context.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, ct);
        public IEnumerable<Booking> GetPending() => _context.Bookings.Where(x => x.Status == BookingStatus.Pending);
        public async Task<int> GetBookingCount(Guid userId) => await _context.Bookings.Where(x => x.UserId == userId && (x.Status== BookingStatus.Confirmed || x.Status == BookingStatus.Pending)).CountAsync();

    }
}
