using EventManager.Application.Interfaces;
using EventManager.ApplicationInterfaces;
using EventManager.Domain.Models;
using EventManager.DomainModels;


namespace EventManager.Application.Services
{
    public class BookingService : IBookingService
    {
        //private List<Booking> _bookingList = new();
        IBookingQueue _queue;
        IEventService _eventService;
        private readonly object _bookingLock = new();
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        IBookingRepository _repository;

        public BookingService(IBookingQueue queue, IEventService eventService, IBookingRepository repository)
        {
            _queue = queue;
            _eventService = eventService;
            _repository = repository;
        }
        public async Task<Booking> CreateBookingAsync(int eventId, Guid userId, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var cheeckAvailability = await _eventService.CheckAvailabilityAsync(eventId, ct);
                if (!cheeckAvailability)
                    throw new NotFoundException("Событие с таким id не существует");

                var booking = new Booking(eventId, BookingStatus.Pending, userId);

                var checkSeats = await _eventService.CheckTryReserveSeatsAsync(eventId, ct);
                if (!checkSeats)
                    throw new NoAvailableSeatsException("Закончились места на событие");

                var eventItem = await _eventService.GetEventAsync(eventId, ct);

                if (eventItem.StartAt < booking.CreatedAt)
                    throw new EventHasEndedException("Нельзя забронировать событие, которое уже началось");

                var userBooking = await _repository.GetBookingCount(userId);

                if(userBooking >=10)
                    throw new BookingLimitExceededException("У пользователя больше 10 броней");

                await _repository.AddBookingAsync(booking, ct);

                await _repository.SaveChangesAsync(ct);

                _queue.Enqueue(booking);

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

        public async Task UpdateBooking(Booking booking, CancellationToken ct = default)
        {
            var bookingItem = await _repository.GetBooking(booking.Id, ct) ?? throw new NotFoundException("Нет брони с таким id");

            bookingItem.Status = booking.Status;
            bookingItem.CreatedAt = booking.CreatedAt;
            bookingItem.ProcessedAt = booking.ProcessedAt;
            bookingItem.EventId = booking.EventId;

            await _repository.SaveChangesAsync(ct);
        }

        public async Task<bool> CancelledBooking(Guid id, Guid userId, RolesEnum role)
        {
            var booking = await _repository.GetBooking(id);

            if(booking.Status != BookingStatus.Cancelled)
            {
                if (role == RolesEnum.Admin || booking.UserId == userId)
                {
                    booking.Status = BookingStatus.Cancelled;
                    await _repository.SaveChangesAsync();

                    return true;
                }
                else
                    throw new NoRightsException("Нет прав на отмену события");

            }
            else
            {
                new Exception("Событие уже было отменено");
            }

            return false;

        }


    }
}
