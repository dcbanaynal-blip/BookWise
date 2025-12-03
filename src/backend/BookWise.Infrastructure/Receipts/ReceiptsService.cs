using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookWise.Application.Receipts;
using BookWise.Domain.Entities;
using BookWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookWise.Infrastructure.Receipts;

public sealed class ReceiptsService : IReceiptsService
{
    private readonly BookWiseDbContext _dbContext;
    private readonly IReceiptFileStorage _fileStorage;
    private readonly IReceiptProcessingQueue _processingQueue;

    public ReceiptsService(BookWiseDbContext dbContext, IReceiptFileStorage fileStorage, IReceiptProcessingQueue processingQueue)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
        _processingQueue = processingQueue;
    }

    public async Task<Receipt> CreateReceiptAsync(CreateReceiptModel model, CancellationToken cancellationToken)
    {
        if (model.FileBytes.Length == 0)
        {
            throw new InvalidOperationException("Receipt file cannot be empty.");
        }

        var storedFile = await _fileStorage.SaveAsync(
            new ReceiptFilePayload(model.OriginalFileName, model.ContentType, model.FileBytes),
            cancellationToken);

        var now = DateTime.UtcNow;
        var receipt = new Receipt
        {
            DocumentDate = model.DocumentDate,
            SellerName = model.SellerName,
            SellerTaxId = model.SellerTaxId,
            SellerAddress = model.SellerAddress,
            CustomerName = model.CustomerName,
            CustomerTaxId = model.CustomerTaxId,
            CustomerAddress = model.CustomerAddress,
            Notes = model.Notes,
            IsVatApplicable = model.IsVatApplicable ?? false,
            MimeType = storedFile.ContentType,
            ImageData = storedFile.Data,
            UploadedBy = model.UploadedBy,
            UploadedAt = now,
            Status = ReceiptStatus.Pending,
            Type = ReceiptType.Unknown
        };

        _dbContext.Receipts.Add(receipt);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _processingQueue.EnqueueAsync(receipt.ReceiptId, cancellationToken);
        return receipt;
    }

    public async Task<IReadOnlyList<Receipt>> GetReceiptsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page <= 0)
        {
            page = 1;
        }

        if (pageSize <= 0 || pageSize > 100)
        {
            pageSize = 50;
        }

        return await _dbContext.Receipts
            .AsNoTracking()
            .OrderByDescending(r => r.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<Receipt?> GetReceiptByIdAsync(int receiptId, CancellationToken cancellationToken)
    {
        return _dbContext.Receipts
            .AsNoTracking()
            .Include(r => r.LineItems)
            .SingleOrDefaultAsync(r => r.ReceiptId == receiptId, cancellationToken);
    }

    public async Task<Receipt> ApproveReceiptAsync(int receiptId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var receipt = await _dbContext.Receipts
            .Include(r => r.Transaction)
            .SingleOrDefaultAsync(r => r.ReceiptId == receiptId, cancellationToken);

        if (receipt is null)
        {
            throw new InvalidOperationException($"Receipt {receiptId} was not found.");
        }

        if (receipt.Status != ReceiptStatus.Completed)
        {
            throw new InvalidOperationException("Receipt OCR must be completed before approval.");
        }

        if (receipt.Transaction is null)
        {
            var now = DateTime.UtcNow;
            var transaction = new FinancialTransaction
            {
                CreatedBy = actorUserId,
                CreatedAt = now,
                TransactionDate = receipt.DocumentDate ?? now,
                Description = receipt.Notes ?? $"Receipt #{receipt.ReceiptId}",
                ReferenceNumber = $"RCPT-{receipt.ReceiptId:D6}",
                Receipt = receipt
            };

            receipt.Transaction = transaction;
            _dbContext.Transactions.Add(transaction);
        }
        else
        {
            receipt.Transaction.TransactionDate = receipt.DocumentDate ?? receipt.Transaction.TransactionDate;
            receipt.Transaction.Description = receipt.Notes ?? receipt.Transaction.Description;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return receipt;
    }
}
