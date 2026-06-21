using EventManager.Models;
using EventManager.Services.Interfaces;

namespace EventManager.Services
{
    public class BookingService : IBookingService
    {
        private List<Booking> _bookingList;
        IBookingQueue _queue;
        IEventService _eventService;

        public BookingService(IBookingQueue queue, IEventService eventService)
        {
            _queue = queue;
            _eventService = eventService;

            _bookingList = new()
            {
                new Booking()
                {
                    EventId = 1,
                    Status = BookingStatus.Pending
                },
                new Booking()
                {
                    EventId = 2,
                    Status = BookingStatus.Pending
                }
            };
        }
        public Booking CreateBookingAsync(int eventId)
        {
            if (!_eventService.CheckAvailability(eventId))
                throw new NotFoundException("Событие с таким id не существует");

            var booking = new Booking()
            {
                EventId = eventId,
                Status = BookingStatus.Pending
            };

            _bookingList.Add(booking);

            _queue.Enqueue(booking);

            return booking;
        }

        public Booking GetBookingByIdAsync(Guid bookingId)
        {
            if (!_bookingList.Any(x => x.Id == bookingId))
                throw new NotFoundException("Брони с таким id не существует");

            var result = _bookingList.Where(x => x.Id == bookingId).FirstOrDefault();

            if (!_eventService.CheckAvailability(result.EventId))
            {
                result.Status = BookingStatus.Rejected;
            }

            return result;
        }

    }
}
