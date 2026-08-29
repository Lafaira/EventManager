
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace BookingService.Booking.Infrastructure.DataAccess.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Domain.Models.Booking>
    {
        public void Configure(EntityTypeBuilder<Domain.Models.Booking> builder)
        {
            builder.ToTable("bookings");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(e => e.Status).HasColumnName("status").IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(e => e.EventId).HasColumnName("event_id").IsRequired();
            builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(e => e.ProcessedAt).HasColumnName("processed_at");
        }
    }
}
