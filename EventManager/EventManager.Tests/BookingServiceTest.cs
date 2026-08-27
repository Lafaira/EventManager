using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.ApplicationInterfaces;
using EventManager.Domain.Models;
using EventManager.DomainModels;
using EventManager.Infrastructure.DataAccess;
using EventManager.Infrastructure.Services;
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
        private readonly Mock<IBookingRepository> _repositoryMock;
        private readonly Mock<IBookingService> _bookingServiceMock;
        private readonly Mock<IAuthService> _authServiceMock;

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
            _repositoryMock = new Mock<IBookingRepository>();
            _bookingService = new BookingService(
                _queueMock.Object,
            _eventServiceMock.Object,
            _repositoryMock.Object
  
        );
            _authServiceMock = new Mock<IAuthService>();

        }
       

        [Fact]
        public async Task CreateBookingAsync_ReturnThrowsNotFoundException()
        {
            int eventId = 6;
            Guid userId = Guid.NewGuid();

            _eventServiceMock
            .Setup(x => x.CheckAvailabilityAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                async () => await _bookingService.CreateBookingAsync(eventId, userId));


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
            Guid userId = Guid.NewGuid();

            _eventServiceMock
                .Setup(x => x.CheckAvailabilityAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _eventServiceMock
                .Setup(x => x.CheckTryReserveSeatsAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => {
                    reserveAttemptsCount++;
                    return reserveAttemptsCount <= 2;
                });

            _eventServiceMock
                .Setup(x => x.GetEventAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => {
                    
                    return new Event(eventId, "Test1", new DateTime(2027, 07, 10), new DateTime(2028, 07, 10), 3);
                });


            var booking1 = await _bookingService.CreateBookingAsync(eventId, userId);
            Assert.NotNull(booking1);
            Assert.Equal(BookingStatus.Pending, booking1.Status);

            var booking2 = await _bookingService.CreateBookingAsync(eventId, userId);
            Assert.NotNull(booking2);
            Assert.NotEqual(booking1.Id, booking2.Id); 

            var exception = await Assert.ThrowsAsync<NoAvailableSeatsException>(
                async () => await _bookingService.CreateBookingAsync(eventId, userId));

            Assert.Equal("No available seats for this event", exception.Message);

            _repositoryMock.Verify(x => x.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _queueMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Exactly(2));
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));

        }


        [Fact]
        public async Task CreateBookingAsync_EventHasEndedException()
        {

            int eventId = 1;
            int reserveAttemptsCount = 0;
            Guid userId = Guid.NewGuid();

            _eventServiceMock
                .Setup(x => x.CheckAvailabilityAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _eventServiceMock
                .Setup(x => x.CheckTryReserveSeatsAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => {
                    reserveAttemptsCount++;
                    return reserveAttemptsCount <= 2;
                });

            _eventServiceMock
                .Setup(x => x.GetEventAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => {

                    return new Event(eventId, "Test1", new DateTime(2025, 07, 10), new DateTime(2028, 07, 10), 3);
                });


            var exception = await Assert.ThrowsAsync<EventHasEndedException>(
                async () => await _bookingService.CreateBookingAsync(eventId, userId));

            Assert.Equal("Cannot book, event completed", exception.Message);

          
        }

        [Fact]
        public async Task CreateBookingAsync_BookingLimitExceededException()
        {

            int eventId = 1;
            int reserveAttemptsCount = 0;
            Guid userId = Guid.NewGuid();

            _eventServiceMock
                .Setup(x => x.CheckAvailabilityAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _eventServiceMock
                .Setup(x => x.CheckTryReserveSeatsAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => {
                    reserveAttemptsCount++;
                    return reserveAttemptsCount <= 2;
                });

            _eventServiceMock
                .Setup(x => x.GetEventAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => {

                    return new Event(eventId, "Test1", new DateTime(2027, 07, 10), new DateTime(2028, 07, 10), 15);
                });

            _repositoryMock
               .Setup(x => x.GetBookingCount(userId))
               .ReturnsAsync(11);

            var exception = await Assert.ThrowsAsync<BookingLimitExceededException>(
                async () => await _bookingService.CreateBookingAsync(eventId, userId));

            Assert.Equal("Booking limit exceeded", exception.Message);

        }
    }
}
