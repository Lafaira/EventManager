using EventManager.Application.Interfaces;
using EventManager.Domain.Models;
using EventManager.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddUser(User user, CancellationToken ct = default) => await _context.Users.AddAsync(user, ct);
        public async Task<User> GetUser(string login, CancellationToken ct = default) => await _context.Users.FirstOrDefaultAsync(x => x.Login == login);
        public async Task SaveChangesAsync(CancellationToken ct = default) => await _context.SaveChangesAsync(ct);

    }
}
