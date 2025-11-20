using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWise.Infrastructure.Persistence.Configurations;

public class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.TransactionId);

        builder.Property(t => t.ReferenceNumber)
            .HasMaxLength(64);

        builder.HasIndex(t => t.ReferenceNumber).IsUnique().HasFilter("[ReferenceNumber] IS NOT NULL");

        builder.Property(t => t.Description)
            .HasMaxLength(255);

        builder.Property(t => t.TransactionDate)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(t => t.Creator)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Receipt)
            .WithOne(r => r.Transaction)
            .HasForeignKey<Receipt>(r => r.TransactionId);
    }
}
