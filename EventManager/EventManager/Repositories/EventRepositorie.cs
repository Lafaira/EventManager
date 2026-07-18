using EventManager.DataAccess;
using EventManager.Models;
using EventManager.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Repositories
{
    public class EventRepositorie : IEventRepositorie
    {
        AppDbContext _context;
        public EventRepositorie(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<Event>> GetAllEventAsync() => _context.Events.AsQueryable();

        public async Task<Event> GetEventAsync(int id, CancellationToken ct = default) => await _context.Events.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task AddEventAsync(Event eventItem, CancellationToken ct = default) => _context.Events.AddAsync(eventItem).AsTask();

        public async Task SaveChangesAsync(CancellationToken ct = default) => await _context.SaveChangesAsync(ct);
        public void Remove(Event eventItem) => _context.Events.Remove(eventItem);

        public async Task<bool> CheckAvailabilityAsync(int id, CancellationToken ct = default) => await _context.Events.AnyAsync(x => x.Id == id, ct);
    }
}
