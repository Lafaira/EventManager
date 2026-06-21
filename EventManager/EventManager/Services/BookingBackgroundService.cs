using EventManager.Services.Interfaces;

namespace EventManager.Services
{
    public class BookingBackgroundService : BackgroundService
    {
        private ILogger<BookingBackgroundService> _logger;
        private IBookingQueue _queue;
        private IBookingService _bookingService;
        public BookingBackgroundService(ILogger<BookingBackgroundService> logger, IBookingQueue queue, IBookingService bookingService)
        {
            _logger = logger;
            _queue = queue;
            _bookingService = bookingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingBackgroundService запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_queue.TryDequeue(out var booking))
                    {
                       
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                        booking.Status = Models.BookingStatus.Confirmed;

                    }
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
    }
}
