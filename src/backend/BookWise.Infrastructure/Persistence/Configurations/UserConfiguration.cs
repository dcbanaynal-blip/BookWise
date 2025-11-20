using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWise.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Role)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnType("varbinary(512)");

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}
