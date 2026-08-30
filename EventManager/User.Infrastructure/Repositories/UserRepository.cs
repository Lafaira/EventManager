using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UserService.User.Application.Interfaces;
using UserService.User.Infrastructure.DataAccess;

namespace UserService.User.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddUser(Domain.Models.User user, CancellationToken ct = default) => await _context.Users.AddAsync(user, ct);
        public async Task<Domain.Models.User> GetUser(string login, CancellationToken ct = default) => await _context.Users.FirstOrDefaultAsync(x => x.Login == login);
        public async Task SaveChangesAsync(CancellationToken ct = default) => await _context.SaveChangesAsync(ct);

    }
}
