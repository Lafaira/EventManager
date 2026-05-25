using EventManager.Models;
using EventManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
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
                    Title = "Title",
                    Description = "Description",
                    StartAt = DateTime.Now,
                    EndAt = DateTime.Now,
                },
                new Event()
                {
                    Id =2,
                    Title = "Title",
                    Description = "Description",
                    StartAt = DateTime.Now,
                    EndAt = DateTime.Now,
                },
            };
        }

        public List<Event> GetAllEvents()
        {
            return _events;
        }

        public Event GetEvent(int id)
        {
            var eventItem = _events.Where(x => x.Id == id).FirstOrDefault() ?? throw new Exception("Нет события с таким id");

            return eventItem;
        }

        public bool PostEvent(Event eventItem)
        {
            var eventItemExist = _events.Where(x => x.Id == eventItem.Id).FirstOrDefault();
            if (eventItemExist != null) throw new Exception("Событие с таким id уже существует");

            _events.Add(eventItem);
            return true;
        }

        public bool PutEvent(int id, Event updatedEvent)
        {
            var eventItem = _events.Where(x =>  x.Id == id).FirstOrDefault() ?? throw new Exception("Нет события с таким id");

            eventItem.Title = updatedEvent.Title;
            eventItem.Description = updatedEvent.Description;
            eventItem.StartAt= updatedEvent.StartAt;
            eventItem.EndAt = updatedEvent.EndAt;

            return true;
        }

        public bool DeleteEvent(int id)
        {
            var eventItem = _events.Where(x => x.Id == id).FirstOrDefault() ?? throw new Exception("Нет события с таким id");

            _events.Remove(eventItem);
            return true;
        }
    }
}
