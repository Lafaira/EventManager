using EventManager.DataAccess;
using EventManager.Models;
using EventManager.Repositories.Interfaces;
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
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        IBookingRepositories _repository;

        public BookingService(IBookingQueue queue, IEventService eventService, IBookingRepositories repository)
        {
            _queue = queue;
            _eventService = eventService;
            _repository = repository;
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

                await _repository.AddBookingAsync(booking, ct);

                _queue.Enqueue(booking);

                await _repository.SaveChangesAsync(ct);

                return booking;
            }
            finally
            {
                _semaphore.Release();
            }
           
        }

        public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default)
        {
            if (! await _repository.IsBookingExist(bookingId, ct))
                throw new NotFoundException("Брони с таким id не существует");

            var result = await _repository.GetBooking(bookingId, ct);

            var cheeckAvailability = await _eventService.CheckAvailabilityAsync(result.EventId, ct);

            if (!cheeckAvailability)
            {
                result.Status = BookingStatus.Rejected;
            }

            return result;
        }

        public IEnumerable<Booking> GetPending()
        {  
            return _repository.GetPending();
        }

    }
}
