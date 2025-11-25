using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWise.Infrastructure.Persistence.Configurations;

public class ReceiptLineItemConfiguration : IEntityTypeConfiguration<ReceiptLineItem>
{
    public void Configure(EntityTypeBuilder<ReceiptLineItem> builder)
    {
        builder.ToTable("ReceiptLineItems");
        builder.HasKey(li => li.ReceiptLineItemId);

        builder.Property(li => li.Quantity)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(li => li.Unit)
            .HasMaxLength(50);

        builder.Property(li => li.Description)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(li => li.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(li => li.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne(li => li.Receipt)
            .WithMany(r => r.LineItems)
            .HasForeignKey(li => li.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
