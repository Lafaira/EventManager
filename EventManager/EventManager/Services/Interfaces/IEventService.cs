using EventManager.Models;
using EventManager.Models.RequestModel;

namespace EventManager.Services.Interfaces
{
    public interface IEventService
    {
        public Task<PaginatedResult> GetAllEventsAsync(PageInfo pageInfo, GetEventsQuery? filterData, CancellationToken ct = default);
        public Task<Event> GetEventAsync(int id, CancellationToken ct = default);
        public Task<Event> PostEventAsync(Event eventItem, CancellationToken ct = default);
        public Task<bool> PutEventAsync(int id, Event updatedEvent, CancellationToken ct = default);
        public Task<bool> DeleteEventAsync(int id, CancellationToken ct = default);
        public Task<bool> CheckAvailabilityAsync(int id, CancellationToken ct = default);
        public Task<bool> CheckTryReserveSeatsAsync(int eventId, CancellationToken ct = default);
        public Task ReleaseSeatsAsync(int id, CancellationToken ct = default);
    }
}
