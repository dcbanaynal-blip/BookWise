using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWise.Infrastructure.Persistence.Configurations;

public class ReceiptProcessingJobConfiguration : IEntityTypeConfiguration<ReceiptProcessingJob>
{
    public void Configure(EntityTypeBuilder<ReceiptProcessingJob> builder)
    {
        builder.ToTable("ReceiptProcessingJobs");
        builder.HasKey(job => job.ReceiptProcessingJobId);

        builder.Property(job => job.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(ReceiptProcessingStatus.Pending)
            .IsRequired();

        builder.Property(job => job.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(job => job.RetryCount)
            .HasDefaultValue(0);

        builder.Property(job => job.ErrorMessage)
            .HasMaxLength(1024);

        builder.HasOne(job => job.Receipt)
            .WithMany()
            .HasForeignKey(job => job.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
