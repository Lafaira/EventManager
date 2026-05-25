using EventManager.Models;

namespace EventManager.Services.Interfaces
{
    public interface IEventService
    {
        public List<Event> GetAllEvents();
        public Event GetEvent(int id);
        public bool PostEvent(Event eventItem);
        public bool PutEvent(int id, Event updatedEvent);
        public bool DeleteEvent(int id);
    }
}
