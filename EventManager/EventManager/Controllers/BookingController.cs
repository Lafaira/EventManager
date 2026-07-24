using EventManager.Application.Dto;
using EventManager.ApplicationInterfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Controllers
{
    [ApiController]
    public class BookingController : Controller
    {
        IBookingService _bookingService;
        public BookingController(IBookingService bookingService) 
        {
            _bookingService = bookingService;
        }

        [HttpPost("events/{id}/book")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PostCreateBooking(int id, CancellationToken ct = default)
        {
            var booking = await _bookingService.CreateBookingAsync(id, ct);

            return AcceptedAtAction(
                actionName: "GetBooking",
                routeValues: new { id = booking.Id },
                value: new BookingDto()
                {
                    Id = booking.Id,
                    EventId = booking.EventId,
                    CreatedAt = booking.CreatedAt,
                    ProcessedAt = booking.ProcessedAt,
                    Status = booking.Status,

                });
        }

        [HttpGet("bookings/{id}")]
        public async Task<IActionResult> GetBooking(Guid id, CancellationToken ct)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id, ct);

            return Ok(booking);
        }

    }
}
