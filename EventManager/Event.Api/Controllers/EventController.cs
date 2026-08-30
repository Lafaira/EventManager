using EventService.Event.Application.Interfaces;
using EventService.Event.Application.Models.Dto;
using EventService.Event.Domain.Models.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EventService.Event.Api.Controllers
{
    [ApiController]
    public class EventController : Controller
    {
        private IEventService _eventService;
        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetAllEvents([FromQuery] GetEventsQuery? filterData, [FromQuery] PageInfo pageInfo, CancellationToken ct = default )
        {
            var events = await _eventService.GetAllEventsAsync(pageInfo, filterData, ct);

            return Ok(events);
        }

        [HttpGet("events/{id}")]
        public async Task<IActionResult> GetEvent(int id, CancellationToken ct = default)
        {
            Domain.Models.Event? eventItem = null;

            eventItem = await _eventService.GetEventAsync(id, ct);
           
            return Ok(eventItem);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("events")]
        public async Task<IActionResult> PostEvent([FromBody] CreateEvent dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ValidationException("Пользователь не заполнил событие");

            var eventItem = new Domain.Models.Event(dto.Id, dto.Title, dto.StartAt, dto.EndAt, dto.TotalSeats, dto.Description);

            var saveEvent = await _eventService.PostEventAsync(eventItem, ct);
           
            return CreatedAtAction(nameof(GetEvent), 
            new { id = saveEvent.Id },
            saveEvent);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("events/{id}")]
        public async Task<IActionResult> PutEvent(int id, [FromBody] CreateEvent dto, CancellationToken ct =default)
        {
            if (dto == null) throw new ValidationException("Пользователь не заполнил событие");

            var updatedEvent = new Domain.Models.Event(dto.Id, dto.Title, dto.StartAt, dto.EndAt, dto.TotalSeats, dto.Description);

            await _eventService.PutEventAsync(id, updatedEvent, ct);


            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("events/{id}")]
        public async Task<IActionResult> DeleteEvent(int id, CancellationToken ct = default)
        {
            await _eventService?.DeleteEventAsync(id, ct);
            
            return Ok();
        }
    }
}
