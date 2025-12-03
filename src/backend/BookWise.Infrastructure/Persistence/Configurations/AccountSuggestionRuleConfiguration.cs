using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWise.Infrastructure.Persistence.Configurations;

public class AccountSuggestionRuleConfiguration : IEntityTypeConfiguration<AccountSuggestionRule>
{
    public void Configure(EntityTypeBuilder<AccountSuggestionRule> builder)
    {
        builder.ToTable("AccountSuggestionRules");
        builder.HasKey(r => r.AccountSuggestionRuleId);

        builder.Property(r => r.SellerName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(r => r.LastUpdatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(r => new { r.SellerName, r.PurposeAccountId, r.PostingAccountId })
            .IsUnique();
    }
}
