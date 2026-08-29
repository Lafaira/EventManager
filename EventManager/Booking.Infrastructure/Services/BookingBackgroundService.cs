using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BookingService.Booking.Application.ApplicationInterfaces;

namespace BookingService.Booking.Infrastructure.Services
{
    public class BookingBackgroundService : BackgroundService
    {
        private ILogger<BookingBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        public BookingBackgroundService(ILogger<BookingBackgroundService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;

            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingBackgroundService запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    List<Domain.Models.Booking> pendingBookings;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                        pendingBookings = bookingService.GetPending().ToList();
                    }

                    var tasks = pendingBookings.Select(async booking =>  await ProcessBookingAsync(booking, stoppingToken));
                    await Task.WhenAll(tasks);

                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении брони");
                }
               

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        private async Task ProcessBookingAsync(Domain.Models.Booking booking, CancellationToken stoppingToken)
        {

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                using (var scope = _scopeFactory.CreateScope())
                {
                    //var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    /*
                    var checkAvailability = await eventService.CheckAvailabilityAsync(booking.EventId, stoppingToken);
                    if (!checkAvailability)
                    {
                        booking.Status = Domain.Models.BookingStatus.Rejected;

                        _logger.LogWarning($"События {booking.EventId} для бронирования нет");
                    }
                    else
                    */
                        booking.Status = Domain.Models.BookingStatus.Confirmed;

                    await bookingService.UpdateBooking(booking, stoppingToken);
                }
  
                
            }
            catch(Exception ex)
            {
                booking.Status = Domain.Models.BookingStatus.Rejected;

                using (var scope = _scopeFactory.CreateScope())
                {
                    //var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    //await eventService.ReleaseSeatsAsync(booking.EventId, stoppingToken);
                    await bookingService.UpdateBooking(booking, stoppingToken);
                }
                    
            }
           
        }
    }
}
