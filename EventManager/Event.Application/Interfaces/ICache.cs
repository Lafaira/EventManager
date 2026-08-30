using System;
using System.Collections.Generic;
using System.Text;

namespace Event.Application.Interfaces
{
    public interface ICache
    {
        Task<EventService.Event.Domain.Models.Event?> GetByIdAsync(int id);
        Task<List<EventService.Event.Domain.Models.Event?>> GetTop10();
        Task RemoveCacheEventById(int id);
    }
}
