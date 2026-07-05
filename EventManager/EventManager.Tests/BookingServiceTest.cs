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
            var eventItem = new Event()
            {
                Id = 1,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01),
                TotalSeats = 3
            };

            var eventresult = _eventService.PostEvent(eventItem);


            var result1 = _bookingService.CreateBookingAsync(eventresult.Id);

            var result2 = _bookingService.CreateBookingAsync(eventresult.Id);

            Assert.Equal(eventresult.Id, result1.EventId);
            Assert.Equal(BookingStatus.Pending, result1.Status);

            Assert.NotEqual(result1.Id, result2.Id);

        }

        [Fact]
        public async Task BookingBackgroundService_ReturnCorrectResult()
        {
            var eventItem = new Event()
            {
                Id = 1,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01),
                TotalSeats = 3

            };

            var resultEvent = _eventService.PostEvent(eventItem);

            var result = _bookingService.CreateBookingAsync(eventItem.Id);


            var service = new BookingBackgroundService(
            _loggerMock.Object,
            _bookingService,
            _eventService);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await service.StartAsync(cts.Token);

            await Task.Delay(TimeSpan.FromSeconds(10));
            var result2 = _bookingService.GetBookingByIdAsync(result.Id);

            Assert.Equal(result2.Status, BookingStatus.Confirmed);
            Assert.Equal(result2.Id, result.Id);

            _bookingService.CreateBookingAsync(eventItem.Id);
            _eventService.DeleteEvent(eventItem.Id);

            var result3 = _bookingService.GetBookingByIdAsync(result.Id);

            Assert.Equal(result3.Status, BookingStatus.Rejected);

            await service.StopAsync(cts.Token);
        }

        [Fact]
        public void CreateBookingAsync_ReturnThrowsNotFoundException()
        {
            int eventId = 6;

            Assert.Equal("Событие с таким id не существует", Assert.Throws<NotFoundException>(() => _bookingService.CreateBookingAsync(eventId)).Message);
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

            Assert.Equal("Брони с таким id не существует", Assert.Throws<NotFoundException>(() => _bookingService.GetBookingByIdAsync(booking.Id)).Message);
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
                EndAt = new DateTime(2026, 01, 01),
                TotalSeats = 3

            };

            var resultEvent = _eventService.PostEvent(eventItem);
            
            var resultBooking = _bookingService.CreateBookingAsync(resultEvent.Id);

            _eventService.DeleteEvent(resultEvent.Id);

            var result = _bookingService.GetBookingByIdAsync(resultBooking.Id);

            Assert.Equal(result.Status, BookingStatus.Rejected);
        }

        [Fact]
        public async Task AvailableSeats_ReturnCorrectResult()
        {

            var eventItem = new Event()
            {
                Id = 1,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01),
                TotalSeats = 2
            };

            var eventresult = _eventService.PostEvent(eventItem);


            var result1 = _bookingService.CreateBookingAsync(eventresult.Id);

            Assert.Equal(eventresult.AvailableSeats, eventItem.TotalSeats-1);

            var result2 = _bookingService.CreateBookingAsync(eventresult.Id);

            Assert.NotEqual(result1.Id, result2.Id);

            Assert.Equal("No available seats for this event", Assert.Throws<NoAvailableSeatsException>(() => _bookingService.CreateBookingAsync(eventresult.Id)).Message);
        }

        [Fact]
        public async Task Overbooking()
        {
            var eventItem = new Event()
            {
                Id = 1,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01),
                TotalSeats = 5
            };

            var eventresult = _eventService.PostEvent(eventItem);

            var service = new BookingBackgroundService(
            _loggerMock.Object,
            _bookingService,
            _eventService);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await service.StartAsync(cts.Token);


            var startSignal = new TaskCompletionSource<bool>();
            var tasks = new List<Task>();

            var correctCount = 0;
            var exceptionCount = 0;
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await startSignal.Task;

                    try
                    {
                        _bookingService.CreateBookingAsync(eventItem.Id);
                        Interlocked.Increment(ref correctCount);
                    }
                    catch (NoAvailableSeatsException ex)
                    {
                        Interlocked.Increment(ref exceptionCount);
                    }
                }));
            }

            startSignal.SetResult(true);

            await Task.WhenAll(tasks);

            Assert.Equal(exceptionCount, 15);
            Assert.Equal(correctCount, 5);
            await service.StopAsync(cts.Token);

        }

        [Fact]
        public async Task UniqueId()
        {
            var eventItem = new Event()
            {
                Id = 1,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01),
                TotalSeats = 10
            };

            var eventresult = _eventService.PostEvent(eventItem);

            var service = new BookingBackgroundService(
            _loggerMock.Object,
            _bookingService,
            _eventService);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await service.StartAsync(cts.Token);


            var startSignal = new TaskCompletionSource<bool>();
            var tasks = new List<Task<Booking?>>();

            var correctCount = 0;
            var exceptionCount = 0;
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => 
                {
                    startSignal.Task.Wait(); 

                    try
                    {
                        var result = _bookingService.CreateBookingAsync(eventItem.Id); 
                        Interlocked.Increment(ref correctCount);
                        return result;
                    }
                    catch (NoAvailableSeatsException)
                    {
                        Interlocked.Increment(ref exceptionCount);
                        return null; 
                    }
                }));
            }

            startSignal.SetResult(true);

            var result = await Task.WhenAll(tasks);

            var uniqueId = result.Select(x => x.Id).ToHashSet();

            Assert.Equal(exceptionCount, 0);
            Assert.Equal(correctCount, 10);
            Assert.Equal(uniqueId.Count, 10);
            await service.StopAsync(cts.Token);

        }


    }
}
