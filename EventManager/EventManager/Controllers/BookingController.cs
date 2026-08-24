using EventManager.Application.Dto;
using EventManager.ApplicationInterfaces;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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

        [Authorize(Roles = "Admin,User")]
        [HttpPost("events/{eventId}/book")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PostCreateBooking( int eventId,  CancellationToken ct = default)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(id, out Guid userId))
            {
                return BadRequest("Не корректный id пользователя");
            }
            var booking = await _bookingService.CreateBookingAsync(eventId, userId, ct);

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

        [Authorize(Roles = "Admin,User")]
        [AllowAnonymous]
        [HttpGet("bookings")]
        public async Task<IActionResult> GetBooking( CancellationToken ct)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(id, out Guid userId))
            {
                return BadRequest("Не корректный id пользователя");
            }
            var booking = await _bookingService.GetBookingByIdAsync(userId, ct);

            return Ok(booking);
        }

        [Authorize(Roles = "Admin,User")]
        [AllowAnonymous]
        [HttpDelete("bookings/{bookingId}")]
        public async Task<IActionResult> CancelledBooking(Guid bookingId, CancellationToken ct)
        {
            var useridString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoleString = User.FindFirstValue(ClaimTypes.Role);

            var userRole = Domain.Models.RolesEnum.User;

            if (useridString == RolesEnum.Admin.ToString())
                userRole = RolesEnum.Admin;


            if (!Guid.TryParse(useridString, out Guid userId))
            {
                return BadRequest("Не корректный id пользователя");
            }

            var isRemove = await _bookingService.CancelledBooking(bookingId, userId, userRole);

            if (!isRemove)
                return BadRequest("Нельзя удалить чужую бронь");
           

            return NoContent();
        }

    }
}
