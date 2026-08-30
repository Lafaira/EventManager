
using EventService.Event.Domain.Models.RequestModel;

namespace EventService.Event.Application.Interfaces
{
    public interface IEventService
    {
        public Task<Domain.Models.PaginatedResult> GetAllEventsAsync(PageInfo pageInfo, GetEventsQuery? filterData, CancellationToken ct = default);
        public Task<Domain.Models.Event> GetEventAsync(int id, CancellationToken ct = default);
        public Task<Domain.Models.Event> PostEventAsync(Domain.Models.Event eventItem, CancellationToken ct = default);
        public Task<bool> PutEventAsync(int id, Domain.Models.Event updatedEvent, CancellationToken ct = default);
        public Task<bool> DeleteEventAsync(int id, CancellationToken ct = default);
        public Task<bool> CheckAvailabilityAsync(int id, CancellationToken ct = default);
        public Task<bool> CheckTryReserveSeatsAsync(int eventId, CancellationToken ct = default);
        public Task ReleaseSeatsAsync(int id, CancellationToken ct = default);
        public Task<List<Domain.Models.Event>?> GetTop10();
    }
}
