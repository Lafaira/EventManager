using EventManager.Models;
using EventManager.Models.RequestModel;
using EventManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Net;

namespace EventManager.Controllers
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
        public async Task<IActionResult> GetAllEvents([FromQuery] GetEventsQuery? filterData, [FromQuery] PageInfo pageInfo )
        {
            var events = _eventService.GetAllEvents(pageInfo, filterData);

            return Ok(events);
        }

        [HttpGet("events/{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            Event? eventItem = null;

            eventItem = _eventService.GetEvent(id);
           
            return Ok(eventItem);
        }

        [HttpPost("events")]
        public async Task<IActionResult> PostEvent([FromBody] Event eventItem)
        {
            if (eventItem == null) throw new NotFoundException("Пользователь не заполнил событие");

            var saveEvent = _eventService.PostEvent(eventItem);
           
            return CreatedAtAction(nameof(GetEvent), 
            new { id = saveEvent.Id },
            saveEvent);
        }

        [HttpPut("events/{id}")]
        public async Task<IActionResult> PutEvent(int id, [FromBody] Event updatedEvent)
        {
            if (updatedEvent == null) throw new NotFoundException("Пользователь не заполнил событие");

            _eventService.PutEvent(id, updatedEvent);


            return Ok();
        }

        [HttpDelete("events/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            _eventService?.DeleteEvent(id);
            
            return Ok();
        }
    }
}
