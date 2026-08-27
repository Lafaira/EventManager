

using EventManager.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace EventManager.Infrastructure.DataAccess.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(e => e.Status).HasColumnName("status").IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.HasOne(e => e.Event).WithMany(e => e.Bookings).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            builder.Property(e => e.EventId).HasColumnName("event_id").IsRequired();
            builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            builder.HasOne(e => e.User).WithMany(e => e.Bookings).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
