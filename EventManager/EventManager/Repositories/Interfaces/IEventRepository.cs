using EventManager.Models;

namespace EventManager.Repositories.Interfaces
{
    public interface IEventRepository
    {
        public Task<IQueryable<Event>> GetAllEventAsync();
        public Task<Event> GetEventAsync(int id, CancellationToken ct = default);
        public Task AddEventAsync(Event eventItem, CancellationToken ct = default);
        public Task SaveChangesAsync(CancellationToken ct = default);
        public void Remove(Event eventItem);
        public Task<bool> CheckAvailabilityAsync(int id, CancellationToken ct = default);
    }
}
