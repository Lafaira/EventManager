using EventManager.Models;
using EventManager.Models.RequestModel;

namespace EventManager.Services.Interfaces
{
    public interface IEventService
    {
        public PaginatedResult GetAllEvents(PageInfo pageInfo, GetEventsQuery? filterData);
        public Event GetEvent(int id);
        public Event PostEvent(Event eventItem);
        public bool PutEvent(int id, Event updatedEvent);
        public bool DeleteEvent(int id);
        public bool CheckAvailability(int id);
    }
}
