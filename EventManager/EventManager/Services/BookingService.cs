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
        public Booking CreateBooking(int eventId)
        {
            if (!_eventService.CheckAvailability(eventId))
                throw new NotFoundException("Событие с таким id не существует");

            var task = new Booking()
            {
                EventId = eventId,
                Status = BookingStatus.Pending
            };

            _queue.Enqueue(task);

            return task;
        }

        public Booking GetBookingById(Guid bookingId)
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

        public bool SaveBooking(Booking booking)
        {
            if (!_eventService.CheckAvailability(booking.EventId))
                return false;

            _bookingList.Add(booking);

            return true;
        }
    }
}
