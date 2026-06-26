using EventManager.Models;
using EventManager.Services.Interfaces;

namespace EventManager.Services
{
    public class BookingBackgroundService : BackgroundService
    {
        private ILogger<BookingBackgroundService> _logger;
        private IBookingService _bookingService;
        private IEventService _eventService;
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
        public BookingBackgroundService(ILogger<BookingBackgroundService> logger, IBookingService bookingService, IEventService eventService)
        {
            _logger = logger;
            _bookingService = bookingService;
            _eventService = eventService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingBackgroundService запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var pendingBookings = _bookingService.GetPending().ToList();
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

                await _processingSemaphore.WaitAsync();

                if (_eventService.CheckAvailability(booking.EventId))
                {
                    booking.Status = Models.BookingStatus.Rejected;
                    _logger.LogWarning($"События {booking.EventId} для бронирования нет");
                }
                    
                booking.Status = Models.BookingStatus.Confirmed;
            }
            catch(Exception ex)
            {
                booking.Status = Models.BookingStatus.Rejected;
                _eventService.ReleaseSeats(booking.EventId);
            }
            finally
            {
                _processingSemaphore.Release();
            }
           
        }
    }
}
