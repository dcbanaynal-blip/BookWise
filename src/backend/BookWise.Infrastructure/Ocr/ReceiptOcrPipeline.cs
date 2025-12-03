using BookWise.Domain.Entities;
using BookWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace BookWise.Infrastructure.Ocr;

public interface IReceiptOcrPipeline
{
    Task<int> PreprocessPendingReceiptsAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<int> ExtractReceiptContentAsync(int batchSize, CancellationToken cancellationToken = default);
}

public class ReceiptOcrPipeline : IReceiptOcrPipeline
{
    private readonly BookWiseDbContext _dbContext;
    private readonly IReceiptImagePreprocessor _preprocessor;
    private readonly ILogger<ReceiptOcrPipeline> _logger;
    private readonly ITesseractEngineFactory _engineFactory;

    public ReceiptOcrPipeline(
        BookWiseDbContext dbContext,
        IReceiptImagePreprocessor preprocessor,
        ITesseractEngineFactory engineFactory,
        ILogger<ReceiptOcrPipeline> logger)
    {
        _dbContext = dbContext;
        _preprocessor = preprocessor;
        _engineFactory = engineFactory;
        _logger = logger;
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

        var now = DateTime.UtcNow;

        foreach (var job in jobs)
        {
            job.Status = ReceiptProcessingStatus.Processing;
            job.StartedAt = now;

            var receipt = job.Receipt;
            if (receipt is null)
            {
                job.Status = ReceiptProcessingStatus.Failed;
                job.CompletedAt = DateTime.UtcNow;
                continue;
            }

            try
            {
                receipt.ImageData = await _preprocessor.NormalizeAsync(receipt.ImageData, cancellationToken);
                receipt.Status = ReceiptStatus.Processing;
                job.Status = ReceiptProcessingStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                job.Status = ReceiptProcessingStatus.Failed;
                job.CompletedAt = DateTime.UtcNow;
                job.RetryCount += 1;
                receipt.Status = ReceiptStatus.Failed;
                _logger.LogError(ex, "Failed preprocessing receipt {ReceiptId}", receipt.ReceiptId);
            }
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

        using var engine = _engineFactory.Create();

        foreach (var job in jobs)
        {
            var receipt = job.Receipt!;
            try
            {
                using var pix = Pix.LoadFromMemory(receipt.ImageData);
                using var page = engine.Process(pix);
                var text = page.GetText();
                var confidence = page.GetMeanConfidence();

                receipt.OcrText = text;
                receipt.Status = ReceiptStatus.Completed;
                receipt.OcrConfidence = confidence;
                job.Status = ReceiptProcessingStatus.Completed;
            }
            catch (Exception ex)
            {
                receipt.Status = ReceiptStatus.Failed;
                job.Status = ReceiptProcessingStatus.Failed;
                job.RetryCount += 1;
                _logger.LogError(ex, "OCR extraction failed for receipt {ReceiptId}", receipt.ReceiptId);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return jobs.Count;
    }
}
