using EventManager.DataAccess;
using EventManager.Models;
using EventManager.Models.RequestModel;
using EventManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Reflection.Metadata.Ecma335;

namespace EventManager.Services
{
    public class EventService : IEventService
    {
        AppDbContext _context;
        public EventService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult> GetAllEventsAsync(PageInfo pageInfo, GetEventsQuery? filterData = null, CancellationToken ct = default)
        {
            var events = _context.Events.AsQueryable();

            if(filterData != null)
            {
                if (!string.IsNullOrWhiteSpace(filterData.Title))
                    events = events.Where(x => x.Title.Contains(filterData.Title, StringComparison.OrdinalIgnoreCase));

                if (filterData.From.HasValue)
                    events = events.Where(x => x.StartAt >= filterData.From);

                if (filterData.To.HasValue)
                    events = events.Where(x => x.EndAt <= filterData.To);
            }

            int countFilterEvent = await events.CountAsync(ct);
            events = events.Skip((pageInfo.Page - 1) * pageInfo.PageSize).Take(pageInfo.PageSize);

            return new PaginatedResult()
            {
                CountEvent= countFilterEvent,
                EventArr = await events.ToArrayAsync(ct), 
                NumberCurrentPage = pageInfo.Page,
                CountEventInPage = events.Count()
            };
        }

        public async Task<Event> GetEventAsync(int id, CancellationToken ct = default)
        {
            var eventItem =  await _context.Events.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Нет события с таким id");

            return eventItem;
        }

        public async Task<Event> PostEventAsync(Event eventItem, CancellationToken ct =default)
        {
            var eventItemExist = _context.Events.Where(x => x.Id == eventItem.Id).FirstOrDefault();
            if (eventItemExist != null) throw new NotFoundException("Событие с таким id уже существует");

            await _context.Events.AddAsync(eventItem);
            await _context.SaveChangesAsync(ct);
            return eventItem;
        }

        public async Task<bool> PutEventAsync(int id, Event updatedEvent, CancellationToken ct)
        {
            var eventItem = await _context.Events.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Нет события с таким id");

            eventItem.Title = updatedEvent.Title;
            eventItem.Description = updatedEvent.Description;
            eventItem.StartAt= updatedEvent.StartAt;
            eventItem.EndAt = updatedEvent.EndAt;

            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> DeleteEventAsync(int id, CancellationToken ct = default)
        {
            var eventItem = await _context.Events.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Нет события с таким id");

            _context.Events.Remove(eventItem);

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CheckAvailabilityAsync(int id, CancellationToken ct = default)
        {
            return await _context.Events.AnyAsync(x => x.Id == id, ct);
        }

        public async Task<bool> CheckTryReserveSeatsAsync(int eventId, CancellationToken ct = default)
        {
            var eventItem = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId, ct);

            if (!eventItem.TryReserveSeats())
                return false;

            return true;
        }

        public async Task ReleaseSeatsAsync(int id, CancellationToken ct = default)
        {
            var eventItem = await _context.Events.FirstOrDefaultAsync(x => x.Id == id, ct);
            eventItem.ReleaseSeats();
        }
    }
}
