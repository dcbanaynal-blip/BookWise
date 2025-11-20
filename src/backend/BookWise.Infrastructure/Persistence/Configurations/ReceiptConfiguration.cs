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

        builder.Property(r => r.ImageData)
            .HasColumnType("varbinary(max)")
            .IsRequired();

        builder.Property(r => r.MimeType)
            .HasMaxLength(80);

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
