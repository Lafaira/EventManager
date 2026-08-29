
namespace EventService.Event.Application.Interfaces
{
    public interface IEventRepository
    {
        public Task<IQueryable<Domain.Models.Event>> GetAllEventAsync();
        public Task<Domain.Models.Event> GetEventAsync(int id, CancellationToken ct = default);
        public Task AddEventAsync(Domain.Models.Event eventItem, CancellationToken ct = default);
        public Task SaveChangesAsync(CancellationToken ct = default);
        public void Remove(Domain.Models.Event eventItem);
        public Task<bool> CheckAvailabilityAsync(int id, CancellationToken ct = default);
        public IQueryable<Domain.Models.Event> SearchStringData(string filterString, IQueryable<Domain.Models.Event> events);
    }
}
