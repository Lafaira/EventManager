using Microsoft.EntityFrameworkCore;

namespace BookingService.Booking.Infrastructure.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Domain.Models.Booking> Bookings => Set<Domain.Models.Booking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
