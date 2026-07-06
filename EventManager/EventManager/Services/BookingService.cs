using EventManager.DataAccess;
using EventManager.Models;
using EventManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Services
{
    public class BookingService : IBookingService
    {
        //private List<Booking> _bookingList = new();
        IBookingQueue _queue;
        IEventService _eventService;
        private readonly object _bookingLock = new();
        private AppDbContext _context;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public BookingService(IBookingQueue queue, IEventService eventService, AppDbContext context)
        {
            _queue = queue;
            _eventService = eventService;
            _context = context;
          
        }
        public async Task<Booking> CreateBookingAsync(int eventId, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var cheeckAvailability = await _eventService.CheckAvailabilityAsync(eventId, ct);
                if (!cheeckAvailability)
                    throw new NotFoundException("Событие с таким id не существует");

                var booking = new Booking(eventId, BookingStatus.Pending);

                var checkSeats = await _eventService.CheckTryReserveSeatsAsync(eventId, ct);
                if (!checkSeats)
                    throw new NoAvailableSeatsException("Закончились места на событие");

                await _context.Bookings.AddAsync(booking, ct);

                _queue.Enqueue(booking);

                await _context.SaveChangesAsync(ct);

                return booking;
            }
            finally
            {
                _semaphore.Release();
            }
           
        }

        public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default)
        {
            if (! await _context.Bookings.AnyAsync(x => x.Id == bookingId, ct))
                throw new NotFoundException("Брони с таким id не существует");

            var result = await _context.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, ct);

            var cheeckAvailability = await _eventService.CheckAvailabilityAsync(result.EventId, ct);

            if (!cheeckAvailability)
            {
                result.Status = BookingStatus.Rejected;
            }

            return result;
        }

        public IEnumerable<Booking> GetPending()
        {  
            return _context.Bookings.Where(x => x.Status == BookingStatus.Pending);
        }

    }
}
