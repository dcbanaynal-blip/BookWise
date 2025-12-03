namespace ReceiptLoadTester;

using BookWise.Domain.Entities;
using BookWise.Infrastructure.Ocr;
using BookWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class SimulatedReceiptOcrPipeline : IReceiptOcrPipeline
{
    private readonly BookWiseDbContext _dbContext;
    private readonly ReceiptLoadOptions _options;

    public SimulatedReceiptOcrPipeline(BookWiseDbContext dbContext, ReceiptLoadOptions options)
    {
        _dbContext = dbContext;
        _options = options;
    }

    public async Task<int> PreprocessPendingReceiptsAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var jobs = await _dbContext.ReceiptProcessingJobs
            .AsTracking()
            .Include(job => job.Receipt)
            .Where(job => job.Status == ReceiptProcessingStatus.Pending)
            .OrderBy(job => job.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (jobs.Count == 0)
        {
            return 0;
        }

        foreach (var job in jobs)
        {
            var receipt = job.Receipt!;
            job.Status = ReceiptProcessingStatus.Processing;
            job.StartedAt ??= DateTime.UtcNow;

            await Task.Delay(_options.PreprocessDelayMs, cancellationToken);

            receipt.Status = ReceiptStatus.Processing;
            job.Status = ReceiptProcessingStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return jobs.Count;
    }

    public async Task<int> ExtractReceiptContentAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var jobs = await _dbContext.ReceiptProcessingJobs
            .AsTracking()
            .Include(job => job.Receipt)
            .Where(job => job.Status == ReceiptProcessingStatus.Completed && job.Receipt!.Status == ReceiptStatus.Processing)
            .OrderBy(job => job.CompletedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (jobs.Count == 0)
        {
            return 0;
        }

        foreach (var job in jobs)
        {
            var receipt = job.Receipt!;
            await Task.Delay(_options.ExtractDelayMs, cancellationToken);

            receipt.OcrText = $"Simulated OCR text #{receipt.ReceiptId}";
            receipt.OcrConfidence = 0.95;
            receipt.Status = ReceiptStatus.Completed;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return jobs.Count;
    }
}
