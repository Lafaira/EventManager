
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace UserService.User.Infrastructure.DataAccess.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<Domain.Models.User>
    {
        public void Configure(EntityTypeBuilder<Domain.Models.User> builder)
        {
            builder.ToTable("users");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(e => e.Login).HasColumnName("login").IsRequired();
            builder.Property(e => e.HashPassword).HasColumnName("hash_password").IsRequired();
            builder.Property(e => e.Roles).HasColumnName("roles").IsRequired();

        }
    }
}
