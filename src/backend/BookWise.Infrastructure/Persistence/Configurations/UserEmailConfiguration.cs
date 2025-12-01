using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWise.Infrastructure.Persistence.Configurations;

public class UserEmailConfiguration : IEntityTypeConfiguration<UserEmail>
{
    public void Configure(EntityTypeBuilder<UserEmail> builder)
    {
        builder.ToTable("UserEmails");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.User)
            .WithMany(u => u.Emails)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Creator)
            .WithMany(u => u.UserEmailsCreated)
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
