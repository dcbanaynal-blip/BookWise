namespace BookWise.Domain.Entities;

public class ReceiptProcessingJob
{
    public int ReceiptProcessingJobId { get; set; }
    public int ReceiptId { get; set; }
    public ReceiptProcessingStatus Status { get; set; } = ReceiptProcessingStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RetryCount { get; set; }

    public Receipt Receipt { get; set; } = null!;
}
