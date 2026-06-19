using EventManager.Models;
using EventManager.Services;
using EventManager.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Tests
{
    public class BookingServiceTest
    {
        private readonly Mock<IBookingQueue> _queueMock;
        //private readonly Mock<IEventService> _eventServiceMock;
        private readonly BookingService _bookingService;
        private readonly Mock<ILogger<BookingBackgroundService>> _loggerMock;
        private readonly EventService _eventService;
        //private readonly BookingQueue _queue;


        public BookingServiceTest()
        {
            _queueMock = new Mock<IBookingQueue>();
            _eventService = new EventService();
            //_eventServiceMock = new Mock<IEventService>();
            //_queue = new BookingQueue();
            _bookingService = new BookingService(_queueMock.Object, _eventService);

            _loggerMock = new Mock<ILogger<BookingBackgroundService>>();

        }

        [Fact]
        public async Task CreateBookingAsync_ReturnCorrectResult()
        {
            int eventId = 1;
            /*
            _eventServiceMock
            .Setup(x => x.CheckAvailability(eventId))
            .Returns(true);
            */
            var result1 = _bookingService.CreateBooking(eventId);
            var result2 = _bookingService.CreateBooking(eventId);

            Assert.Equal(eventId, result1.EventId);
            Assert.Equal(BookingStatus.Pending, result1.Status);

            Assert.NotEqual(result1.Id, result2.Id);

        }

        [Fact]
        public async Task BookingBackgroundService_ReturnCorrectResult()
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                EventId = 1,
                Status = BookingStatus.Pending
            };

            var queue = new BookingQueue();
            queue.Enqueue(booking);

            var service = new BookingBackgroundService(
            _loggerMock.Object,
            queue,
            _bookingService);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await service.StartAsync(cts.Token);

            await Task.Delay(TimeSpan.FromSeconds(10));
            var result3 = _bookingService.GetBookingById(booking.Id);

            Assert.Equal(result3.Status, BookingStatus.Confirmed);
            Assert.Equal(result3.Id, booking.Id);

            await service.StopAsync(cts.Token);
        }

        [Fact]
        public void CreateBookingAsync_ReturnThrowsNotFoundException()
        {
            int eventId = 6;

            Assert.Equal("Событие с таким id не существует", Assert.Throws<NotFoundException>(() => _bookingService.CreateBooking(eventId)).Message);
        }

        [Fact]
        public void GetBookingByIdAsync_ReturnThrowsNotFoundException()
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                EventId = 1,
                Status = BookingStatus.Pending
            };

            Assert.Equal("Брони с таким id не существует", Assert.Throws<NotFoundException>(() => _bookingService.GetBookingById(booking.Id)).Message);
        }

        [Fact]
        public async Task GetBookingByIdAsync_BookingStatusRejected()
        {
            var eventItem = new Event()
            {
                Id = 5,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01)
            };

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventItem.Id,
                Status = BookingStatus.Pending
            };

            var resultEvent = _eventService.PostEvent(eventItem);

             _bookingService.SaveBooking(booking);

            _eventService.DeleteEvent(resultEvent.Id);

            var result = _bookingService.GetBookingById(booking.Id);

            Assert.Equal(result.Status, BookingStatus.Rejected);
        }
    }
}
