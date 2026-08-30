using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventService.Event.Infrastructure.DataAccess.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Domain.Models.Event>
    {
        public void Configure(EntityTypeBuilder<Domain.Models.Event> builder)
        {
            builder.ToTable("events");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
            builder.Property(e => e.StartAt).HasColumnName("start_at").IsRequired();
            builder.Property(e => e.EndAt).HasColumnName("end_at").IsRequired();
            builder.Property(e => e.TotalSeats).HasColumnName("total_seats");
            builder.Property(e => e.AvailableSeats).HasColumnName("available_seats").IsRequired();
            builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(2000);
        }
    }
}
