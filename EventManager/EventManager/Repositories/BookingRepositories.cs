using EventManager.DataAccess;
using EventManager.Models;
using EventManager.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Repositories
{
    public class BookingRepositories : IBookingRepositories
    {
        AppDbContext _context;
        public BookingRepositories(AppDbContext context) 
        {
            _context = context;
        }

        public async Task AddBookingAsync(Booking item, CancellationToken ct = default) => await _context.Bookings.AddAsync(item, ct);
        public async Task SaveChangesAsync(CancellationToken ct = default) => await _context.SaveChangesAsync(ct);
        public Task<bool> IsBookingExist(Guid bookingId, CancellationToken ct = default) => _context.Bookings.AnyAsync(x => x.Id == bookingId, ct);
        public async Task<Booking> GetBooking(Guid bookingId, CancellationToken ct = default) => await _context.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, ct);
        public IEnumerable<Booking> GetPending() => _context.Bookings.Where(x => x.Status == BookingStatus.Pending);
    }
}
