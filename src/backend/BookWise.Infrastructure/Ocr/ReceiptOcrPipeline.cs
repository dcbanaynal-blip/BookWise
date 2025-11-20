using BookWise.Domain.Entities;
using BookWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookWise.Infrastructure.Ocr;

public interface IReceiptOcrPipeline
{
    Task<int> PreprocessPendingReceiptsAsync(int batchSize, CancellationToken cancellationToken = default);
}

public class ReceiptOcrPipeline : IReceiptOcrPipeline
{
    private readonly BookWiseDbContext _dbContext;
    private readonly IReceiptImagePreprocessor _preprocessor;
    private readonly ILogger<ReceiptOcrPipeline> _logger;

    public ReceiptOcrPipeline(
        BookWiseDbContext dbContext,
        IReceiptImagePreprocessor preprocessor,
        ILogger<ReceiptOcrPipeline> logger)
    {
        _dbContext = dbContext;
        _preprocessor = preprocessor;
        _logger = logger;
    }

    public async Task<int> PreprocessPendingReceiptsAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var receipts = await _dbContext.Receipts
            .Where(r => r.Status == ReceiptStatus.Pending)
            .OrderBy(r => r.ReceiptId)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        foreach (var receipt in receipts)
        {
            try
            {
                receipt.ImageData = await _preprocessor.NormalizeAsync(receipt.ImageData, cancellationToken);
                receipt.Status = ReceiptStatus.Processing;
            }
            catch (Exception ex)
            {
                receipt.Status = ReceiptStatus.Failed;
                _logger.LogError(ex, "Failed preprocessing receipt {ReceiptId}", receipt.ReceiptId);
            }
        }

        if (receipts.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return receipts.Count;
    }
}
