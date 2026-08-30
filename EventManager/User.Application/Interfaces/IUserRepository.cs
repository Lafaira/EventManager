using System;
using System.Collections.Generic;
using System.Text;

namespace UserService.User.Application.Interfaces
{
    public interface IUserRepository
    {
        public Task AddUser(Domain.Models.User user, CancellationToken ct = default);
        public Task<Domain.Models.User> GetUser(string login, CancellationToken ct = default);
        public Task SaveChangesAsync(CancellationToken ct = default);
    }
}
