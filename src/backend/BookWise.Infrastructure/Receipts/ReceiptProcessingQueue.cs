using System;
using System.Threading;
using System.Threading.Tasks;
using BookWise.Application.Receipts;
using BookWise.Domain.Entities;
using BookWise.Infrastructure.Persistence;

namespace BookWise.Infrastructure.Receipts;

public sealed class ReceiptProcessingQueue : IReceiptProcessingQueue
{
    private readonly BookWiseDbContext _dbContext;

    public ReceiptProcessingQueue(BookWiseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnqueueAsync(int receiptId, CancellationToken cancellationToken)
    {
        var job = new ReceiptProcessingJob
        {
            ReceiptId = receiptId,
            Status = ReceiptProcessingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0
        };

        _dbContext.ReceiptProcessingJobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
