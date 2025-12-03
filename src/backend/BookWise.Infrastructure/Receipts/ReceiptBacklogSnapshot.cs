using System;

namespace BookWise.Infrastructure.Receipts;

public sealed record ReceiptBacklogSnapshot
{
    public DateTime CapturedAtUtc { get; init; }
    public int PendingReceipts { get; init; }
    public int ProcessingReceipts { get; init; }
    public int AwaitingReviewReceipts { get; init; }
    public int FailedReceipts { get; init; }
    public int PendingJobs { get; init; }
    public int ProcessingJobs { get; init; }
    public int FailedJobs { get; init; }
    public TimeSpan? OldestPendingReceiptAge { get; init; }
    public TimeSpan? OldestPendingJobAge { get; init; }
    public TimeSpan? OldestProcessingJobAge { get; init; }
}

public sealed record ReceiptBacklogStatus
{
    public ReceiptBacklogSnapshot Snapshot { get; init; } = new();
    public bool PendingReceiptAlert { get; init; }
    public bool PendingJobAlert { get; init; }
    public bool ProcessingJobAlert { get; init; }
    public bool HasAlert => PendingReceiptAlert || PendingJobAlert || ProcessingJobAlert;
}
