using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWise.Infrastructure.Persistence.Configurations;

public class ReceiptDecisionConfiguration : IEntityTypeConfiguration<ReceiptDecision>
{
    public void Configure(EntityTypeBuilder<ReceiptDecision> builder)
    {
        builder.ToTable("ReceiptDecisions");
        builder.HasKey(d => d.ReceiptDecisionId);

        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(d => d.Notes)
            .HasMaxLength(1024);

        builder.Property(d => d.TotalOverride)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(d => d.Receipt)
            .WithMany(r => r.Decisions)
            .HasForeignKey(d => d.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
