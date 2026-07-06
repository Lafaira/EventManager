using EventManager.DataAccess;
using EventManager.Models;
using EventManager.Services.Interfaces;

namespace EventManager.Services
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
                    List<Booking> pendingBookings;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();
                        pendingBookings = bookingService.GetPending().ToList();
                    }

                    var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));
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

        private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
        {

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

                    if (await eventService.CheckAvailabilityAsync(booking.EventId, stoppingToken))
                    {
                        booking.Status = Models.BookingStatus.Rejected;
                        _logger.LogWarning($"События {booking.EventId} для бронирования нет");
                    }
                }
  
                booking.Status = Models.BookingStatus.Confirmed;
            }
            catch(Exception ex)
            {
                booking.Status = Models.BookingStatus.Rejected;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
                    await eventService.ReleaseSeatsAsync(booking.EventId, stoppingToken);
                }
                    
            }
           
        }
    }
}
