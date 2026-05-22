using EventManager.Models;
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
        public async Task<IActionResult> GetAllEvents()
        {
            var events = _eventService.GetAllEvents();

            return Ok( new ApiResult<List<Event>>()
            {
                Data = events,
                Success =true,
                StatusCode = HttpStatusCode.OK,
                Message = string.Empty
            });
        }

        [HttpGet("events/{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            Event? eventItem = null;

            try
            {
                eventItem = _eventService.GetEvent(id);
            }
            catch (Exception ex)
            {
                return NotFound( new ApiResult()
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = ex.Message
                });
            }

            return Ok( new ApiResult<Event>()
            {
                Data = eventItem,
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = string.Empty
            });
        }

        [HttpPost("events")]
        public async Task<IActionResult> PostEvent([FromBody] Event eventItem)
        {
            try
            {
                if (eventItem == null) throw new Exception("Пользователь не заполнил событие");

                _eventService.PostEvent(eventItem);
            }
            catch (Exception ex)
            {
                return NotFound( new ApiResult()
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = ex.Message
                });
            }

            return Created();
        }

        [HttpPut("events/{id}")]
        public async Task<IActionResult> PutEvent(int id, [FromBody] Event updatedEvent)
        {
            try
            {
                if (updatedEvent == null) throw new Exception("Пользователь не заполнил событие");

                _eventService.PutEvent(id, updatedEvent);
            }
            catch (Exception ex)
            {
                return NotFound( new ApiResult()
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = ex.Message
                });
            }

            return Ok( new ApiResult()
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = string.Empty
            });
        }

        [HttpDelete("events/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                _eventService?.DeleteEvent(id);
            }
            catch (Exception ex)
            {
                return NotFound( new ApiResult()
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = ex.Message
                });
            }

            return Ok(new ApiResult()
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = string.Empty
            });
        }
    }
}
