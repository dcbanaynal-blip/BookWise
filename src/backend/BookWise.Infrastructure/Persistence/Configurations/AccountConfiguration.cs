using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWise.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.AccountId);

        builder.Property(a => a.ExternalAccountNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.SegmentCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Level)
            .IsRequired();

        builder.HasIndex(a => a.ExternalAccountNumber).IsUnique();
        builder.HasIndex(a => a.ParentAccountId);
        builder.HasIndex(a => a.SegmentCode)
            .HasDatabaseName("IX_Accounts_RootSegmentCode")
            .IsUnique()
            .HasFilter("[ParentAccountId] IS NULL");

        builder.HasIndex(a => new { a.ParentAccountId, a.SegmentCode })
            .HasDatabaseName("IX_Accounts_ParentSegmentCode")
            .IsUnique()
            .HasFilter("[ParentAccountId] IS NOT NULL");

        builder.HasCheckConstraint("CK_Accounts_LevelPositive", "[Level] >= 1");

        builder.Property(a => a.ExternalAccountNumber)
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

        builder.Property(a => a.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(a => a.Type)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.HasOne(a => a.ParentAccount)
            .WithMany(a => a.Children)
            .HasForeignKey(a => a.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
