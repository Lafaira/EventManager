
using Microsoft.EntityFrameworkCore;
using UserService.User.Domain.Models;

namespace UserService.User.Infrastructure.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Domain.Models.User> Users => Set<Domain.Models.User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
