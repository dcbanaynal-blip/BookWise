using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWise.Infrastructure.Persistence.Configurations;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("Receipts");
        builder.HasKey(r => r.ReceiptId);

        builder.Property(r => r.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ReceiptType.Unknown)
            .IsRequired();

        builder.Property(r => r.ImageData)
            .HasColumnType("varbinary(max)")
            .IsRequired();

        builder.Property(r => r.MimeType)
            .HasMaxLength(80);

        builder.Property(r => r.DocumentDate);

        builder.Property(r => r.SellerName)
            .HasMaxLength(255);

        builder.Property(r => r.SellerTaxId)
            .HasMaxLength(50);

        builder.Property(r => r.SellerAddress)
            .HasMaxLength(512);

        builder.Property(r => r.CustomerName)
            .HasMaxLength(255);

        builder.Property(r => r.CustomerTaxId)
            .HasMaxLength(50);

        builder.Property(r => r.CustomerAddress)
            .HasMaxLength(512);

        builder.Property(r => r.Terms)
            .HasMaxLength(120);

        builder.Property(r => r.PurchaseOrderNumber)
            .HasMaxLength(120);

        builder.Property(r => r.BusinessStyle)
            .HasMaxLength(120);

        builder.Property(r => r.CurrencyCode)
            .HasMaxLength(3);

        builder.Property(r => r.Notes)
            .HasMaxLength(1024);

        builder.Property(r => r.SubtotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.WithholdingTaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.IsVatApplicable)
            .HasDefaultValue(false);

        builder.Property(r => r.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.UploadedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(r => r.Uploader)
            .WithMany(u => u.UploadedReceipts)
            .HasForeignKey(r => r.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
