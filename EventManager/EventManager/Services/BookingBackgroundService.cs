using EventManager.Services.Interfaces;

namespace EventManager.Services
{
    public class BookingBackgroundService : BackgroundService
    {
        private ILogger<BookingBackgroundService> _logger;
        private IBookingQueue _queue;
        private IBookingService _bookingServicecs;
        public BookingBackgroundService(ILogger<BookingBackgroundService> logger, IBookingQueue queue, IBookingService bookingServicecs)
        {
            _logger = logger;
            _queue = queue;
            _bookingServicecs = bookingServicecs;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingBackgroundService запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_queue.TryDequeue(out var task))
                    {
                       
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                        var isCreate =_bookingServicecs.SaveBooking(task);

                        if(isCreate)
                        {
                            task.Status = Models.BookingStatus.Confirmed;
                            task.ProcessedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            task.Status = Models.BookingStatus.Rejected;
                            task.ProcessedAt = DateTime.UtcNow;
                        }


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
