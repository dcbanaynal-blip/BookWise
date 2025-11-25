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

        builder.Property(u => u.FirstName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(u => u.CreatedBy)
            .IsRequired();

        builder.HasOne(u => u.CreatedByUser)
            .WithMany(u => u.InvitedUsers)
            .HasForeignKey(u => u.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
