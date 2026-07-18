using EventManager.DataAccess;
using EventManager.Models;
using EventManager.Repositories.Interfaces;
using EventManager.Services;
using EventManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Tests
{
    public class BookingServiceTest
    {
        private readonly Mock<IBookingQueue> _queueMock;
        private readonly IBookingService _bookingService;
        private readonly Mock<ILogger<BookingBackgroundService>> _loggerMock;
        private readonly IEventService _eventService;
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;

        private readonly Mock<IEventService> _eventServiceMock;
        private readonly Mock<IBookingRepositories> _repositoryMock;
        private readonly Mock<IBookingService> _bookingServiceMock;

        public BookingServiceTest()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddSingleton<IBookingQueue, BookingQueue>();

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            //_eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
            //_bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();


            _queueMock = new Mock<IBookingQueue>();

            _loggerMock = new Mock<ILogger<BookingBackgroundService>>();

            _eventServiceMock = new Mock<IEventService>();
            _repositoryMock = new Mock<IBookingRepositories>();
            _bookingService = new BookingService(
                _queueMock.Object,
            _eventServiceMock.Object,
            _repositoryMock.Object
            
        );






        }
        /*
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

            var mockRepo = new Mock<IEventRepositorie>();
            mockRepo.Setup(r => r.AddEventAsync(eventItem));

            var eventresult = await _eventService.PostEventAsync(eventItem);


            var result1 = await _bookingService.CreateBookingAsync(eventresult.Id);

            var result2 = await _bookingService.CreateBookingAsync(eventresult.Id);

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

            var resultEvent = await _eventService.PostEventAsync(eventItem);

            var result = await _bookingService.CreateBookingAsync(eventItem.Id);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await Task.Delay(TimeSpan.FromSeconds(10));
            var result2 = await _bookingService.GetBookingByIdAsync(result.Id);

            Assert.Equal(result2.Id, result.Id);

        }
        */

        [Fact]
        public async Task CreateBookingAsync_ReturnThrowsNotFoundException()
        {
            int eventId = 6;

            //var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(eventId));
            _eventServiceMock
            .Setup(x => x.CheckAvailabilityAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                async () => await _bookingService.CreateBookingAsync(eventId));


            Assert.Equal("Событие с таким id не существует", exception.Message);

        }

        [Fact]
        public async Task GetBookingByIdAsync_ReturnThrowsNotFoundException()
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                EventId = 1,
                Status = BookingStatus.Pending
            };

            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await _bookingService.GetBookingByIdAsync(booking.Id));

            Assert.Equal("Брони с таким id не существует", exception.Message);

           
        }

        

        [Fact]
        public async Task CreateBookingAsync()
        {
            
            int eventId = 1;
            int reserveAttemptsCount = 0;

            _eventServiceMock
                .Setup(x => x.CheckAvailabilityAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _eventServiceMock
                .Setup(x => x.CheckTryReserveSeatsAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => {
                    reserveAttemptsCount++;
                    return reserveAttemptsCount <= 2;
                });


            var booking1 = await _bookingService.CreateBookingAsync(eventId);
            Assert.NotNull(booking1);
            Assert.Equal(BookingStatus.Pending, booking1.Status);

            var booking2 = await _bookingService.CreateBookingAsync(eventId);
            Assert.NotNull(booking2);
            Assert.NotEqual(booking1.Id, booking2.Id); 

            var exception = await Assert.ThrowsAsync<NoAvailableSeatsException>(
                async () => await _bookingService.CreateBookingAsync(eventId));

            Assert.Equal("No available seats for this event", exception.Message);

            _repositoryMock.Verify(x => x.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _queueMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Exactly(2));
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));


        }

        /*
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

            var eventresult = await _eventService.PostEventAsync(eventItem);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));


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
                        using var scope = _serviceProvider.CreateScope();
                        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                        await bookingService.CreateBookingAsync(eventItem.Id);

                        //_bookingService.CreateBookingAsync(eventItem.Id);
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
            //await service.StopAsync(cts.Token);

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

            var eventresult = await _eventService.PostEventAsync(eventItem);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            //await service.StartAsync(cts.Token);


            var startSignal = new TaskCompletionSource<bool>();
            var tasks = new List<Task<Booking?>>();

            var correctCount = 0;
            var exceptionCount = 0;
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run( async() => 
                {
                    startSignal.Task.Wait(); 

                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                        var result = await bookingService.CreateBookingAsync(eventItem.Id);
                        //bookingIds.Add(booking.Id);

                        //var result = _bookingService.CreateBookingAsync(eventItem.Id); 
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
            //await service.StopAsync(cts.Token);

        }
        */
        
    }
}
