namespace BookWise.Infrastructure.Receipts;

public sealed class ReceiptBacklogMonitorOptions
{
    public int SnapshotIntervalMinutes { get; set; } = 5;
    public int PendingReceiptAlertMinutes { get; set; } = 30;
    public int PendingJobAlertMinutes { get; set; } = 20;
    public int ProcessingJobAlertMinutes { get; set; } = 10;
}
