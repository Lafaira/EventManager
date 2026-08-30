using EventService.Event.Application.Interfaces;
using EventService.Event.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventService.Event.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        AppDbContext _context;
        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<Domain.Models.Event>> GetAllEventAsync() => _context.Events.AsQueryable();

        public async Task<Domain.Models.Event> GetEventAsync(int id, CancellationToken ct = default) => await _context.Events.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task AddEventAsync(Domain.Models.Event eventItem, CancellationToken ct = default) => _context.Events.AddAsync(eventItem).AsTask();

        public async Task SaveChangesAsync(CancellationToken ct = default) => await _context.SaveChangesAsync(ct);
        public void Remove(Domain.Models.Event eventItem) => _context.Events.Remove(eventItem);

        public async Task<bool> CheckAvailabilityAsync(int id, CancellationToken ct = default) => await _context.Events.AnyAsync(x => x.Id == id, ct);
        public IQueryable<Domain.Models.Event> SearchStringData(string filterString, IQueryable<Domain.Models.Event> events)
        {
            var searchPattern = $"%{filterString}%";
            return events.Where(x => EF.Functions.ILike(x.Title, searchPattern));
        }

    }
}
