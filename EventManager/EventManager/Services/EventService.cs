using EventManager.Models;
using EventManager.Models.RequestModel;
using EventManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Reflection.Metadata.Ecma335;

namespace EventManager.Services
{
    public class EventService : IEventService
    {
        List<Event> _events;
        public EventService()
        {
            _events = new List<Event>()
            {
                new Event()
                {
                    Id =1,
                    Title = "Title1",
                    Description = "Description",
                    StartAt = new DateTime(2026, 09, 11),
                    EndAt = new DateTime(2026, 10, 11),
                },
                new Event()
                {
                    Id =2,
                    Title = "Title2",
                    Description = "Description",
                    StartAt = new DateTime(2027, 09, 11),
                    EndAt = new DateTime(2028, 09, 11),
                },
            };
        }

        public PaginatedResult GetAllEvents(PageInfo pageInfo, GetEventsQuery? filterData = null)
        {
            IEnumerable<Event> events = _events;

            if(filterData != null)
            {
                if (!string.IsNullOrWhiteSpace(filterData.Title))
                    events = events.Where(x => x.Title.Contains(filterData.Title, StringComparison.OrdinalIgnoreCase));

                if (filterData.From.HasValue)
                    events = events.Where(x => x.StartAt >= filterData.From);

                if (filterData.To.HasValue)
                    events = events.Where(x => x.EndAt <= filterData.To);
            }

            int countFilterEvent = events.Count();
            events = events.Skip((pageInfo.Page - 1) * pageInfo.PageSize).Take(pageInfo.PageSize);

            return new PaginatedResult()
            {
                CountEvent= countFilterEvent,
                EventArr = events.ToArray(), 
                NumberCurrentPage = pageInfo.Page,
                CountEventInPage = events.Count()
            };
        }

        public Event GetEvent(int id)
        {
            var eventItem = _events.Where(x => x.Id == id).FirstOrDefault() ?? throw new NotFoundException("Нет события с таким id");

            return eventItem;
        }

        public Event PostEvent(Event eventItem)
        {
            var eventItemExist = _events.Where(x => x.Id == eventItem.Id).FirstOrDefault();
            if (eventItemExist != null) throw new NotFoundException("Событие с таким id уже существует");

            _events.Add(eventItem);
            return eventItem;
        }

        public bool PutEvent(int id, Event updatedEvent)
        {
            var eventItem = _events.Where(x =>  x.Id == id).FirstOrDefault() ?? throw new NotFoundException("Нет события с таким id");

            eventItem.Title = updatedEvent.Title;
            eventItem.Description = updatedEvent.Description;
            eventItem.StartAt= updatedEvent.StartAt;
            eventItem.EndAt = updatedEvent.EndAt;

            return true;
        }

        public bool DeleteEvent(int id)
        {
            var eventItem = _events.Where(x => x.Id == id).FirstOrDefault() ?? throw new NotFoundException("Нет события с таким id");

            _events.Remove(eventItem);
            return true;
        }

        public bool CheckAvailability(int id)
        {
            return _events.Any(x => x.Id == id);
        }
    }
}
