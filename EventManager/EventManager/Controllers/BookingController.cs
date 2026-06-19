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
        IBookingService _bookingServicecs;
        public BookingController(IBookingService bookingServicecs) 
        {
            _bookingServicecs = bookingServicecs;
        }

        [HttpPost("events/{id}/book")]
        public IActionResult PostCreateBooking(int id)
        {
            var booking = _bookingServicecs.CreateBooking(id);

            if (booking == null)
                return NotFound();

            return AcceptedAtAction(
                actionName: "GetBooking",
                routeValues: new { id = booking.Id },
                value: booking);
        }

        [HttpGet("bookings/{id}")]
        public IActionResult GetBooking(Guid id)
        {
            var booking = _bookingServicecs.GetBookingById(id);

            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

    }
}
