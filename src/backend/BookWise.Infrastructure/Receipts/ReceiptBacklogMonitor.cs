using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookWise.Domain.Entities;
using BookWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookWise.Infrastructure.Receipts;

public interface IReceiptBacklogMonitor
{
    Task<ReceiptBacklogStatus> GetStatusAsync(CancellationToken cancellationToken);
}

public sealed class ReceiptBacklogMonitor : IReceiptBacklogMonitor
{
    private readonly BookWiseDbContext _dbContext;
    private readonly ReceiptBacklogMonitorOptions _options;

    public ReceiptBacklogMonitor(
        BookWiseDbContext dbContext,
        IOptions<ReceiptBacklogMonitorOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<ReceiptBacklogStatus> GetStatusAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var pendingReceiptsQuery = _dbContext.Receipts
            .AsNoTracking()
            .Where(r => r.Status == ReceiptStatus.Pending);

        var processingReceiptsQuery = _dbContext.Receipts
            .AsNoTracking()
            .Where(r => r.Status == ReceiptStatus.Processing);

        var failedReceiptsQuery = _dbContext.Receipts
            .AsNoTracking()
            .Where(r => r.Status == ReceiptStatus.Failed);

        var awaitingReviewQuery = _dbContext.Receipts
            .AsNoTracking()
            .Where(r => r.Status == ReceiptStatus.Completed && !r.Decisions.Any());

        var pendingReceipts = await pendingReceiptsQuery.CountAsync(cancellationToken);
        var processingReceipts = await processingReceiptsQuery.CountAsync(cancellationToken);
        var failedReceipts = await failedReceiptsQuery.CountAsync(cancellationToken);
        var awaitingReviewReceipts = await awaitingReviewQuery.CountAsync(cancellationToken);

        var oldestPendingReceiptAt = pendingReceipts > 0
            ? await pendingReceiptsQuery.MinAsync(r => (DateTime?)r.UploadedAt, cancellationToken)
            : null;

        var jobsQuery = _dbContext.ReceiptProcessingJobs.AsNoTracking();
        var pendingJobsQuery = jobsQuery.Where(j => j.Status == ReceiptProcessingStatus.Pending);
        var processingJobsQuery = jobsQuery.Where(j => j.Status == ReceiptProcessingStatus.Processing);

        var pendingJobs = await pendingJobsQuery.CountAsync(cancellationToken);
        var processingJobs = await processingJobsQuery.CountAsync(cancellationToken);
        var failedJobs = await jobsQuery.Where(j => j.Status == ReceiptProcessingStatus.Failed)
            .CountAsync(cancellationToken);

        var oldestPendingJobAt = pendingJobs > 0
            ? await pendingJobsQuery.MinAsync(j => (DateTime?)j.CreatedAt, cancellationToken)
            : null;

        var oldestProcessingJobAt = processingJobs > 0
            ? await processingJobsQuery.MinAsync(j => (DateTime?)(j.StartedAt ?? j.CreatedAt), cancellationToken)
            : null;

        var snapshot = new ReceiptBacklogSnapshot
        {
            CapturedAtUtc = now,
            PendingReceipts = pendingReceipts,
            ProcessingReceipts = processingReceipts,
            AwaitingReviewReceipts = awaitingReviewReceipts,
            FailedReceipts = failedReceipts,
            PendingJobs = pendingJobs,
            ProcessingJobs = processingJobs,
            FailedJobs = failedJobs,
            OldestPendingReceiptAge = oldestPendingReceiptAt.HasValue
                ? now - oldestPendingReceiptAt.Value
                : null,
            OldestPendingJobAge = oldestPendingJobAt.HasValue
                ? now - oldestPendingJobAt.Value
                : null,
            OldestProcessingJobAge = oldestProcessingJobAt.HasValue
                ? now - oldestProcessingJobAt.Value
                : null
        };

        return new ReceiptBacklogStatus
        {
            Snapshot = snapshot,
            PendingReceiptAlert = snapshot.OldestPendingReceiptAge.HasValue &&
                snapshot.OldestPendingReceiptAge.Value.TotalMinutes >= _options.PendingReceiptAlertMinutes,
            PendingJobAlert = snapshot.OldestPendingJobAge.HasValue &&
                snapshot.OldestPendingJobAge.Value.TotalMinutes >= _options.PendingJobAlertMinutes,
            ProcessingJobAlert = snapshot.OldestProcessingJobAge.HasValue &&
                snapshot.OldestProcessingJobAge.Value.TotalMinutes >= _options.ProcessingJobAlertMinutes
        };
    }
}
