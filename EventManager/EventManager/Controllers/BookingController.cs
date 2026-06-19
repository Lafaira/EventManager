using EventManager.Models;
using EventManager.Services;
using EventManager.Services.Interfaces;
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
        public IActionResult PostCreateBooking(int id)
        {
            var booking = _bookingService.CreateBookingAsync(id);

            return AcceptedAtAction(
                actionName: "GetBooking",
                routeValues: new { id = booking.Id },
                value: booking);
        }

        [HttpGet("bookings/{id}")]
        public IActionResult GetBooking(Guid id)
        {
            var booking = _bookingService.GetBookingByIdAsync(id);

            return Ok(booking);
        }

    }
}
