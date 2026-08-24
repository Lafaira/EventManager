using EventManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IUserRepository
    {
        public Task AddUser(User user, CancellationToken ct = default);
        public Task<User> GetUser(string login, CancellationToken ct = default);
        public Task SaveChangesAsync(CancellationToken ct = default);
    }
}
