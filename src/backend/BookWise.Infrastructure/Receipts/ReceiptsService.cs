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

    public ReceiptsService(BookWiseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Receipt> CreateReceiptAsync(CreateReceiptModel model, CancellationToken cancellationToken)
    {
        if (model.FileBytes.Length == 0)
        {
            throw new InvalidOperationException("Receipt file cannot be empty.");
        }

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
            MimeType = model.ContentType,
            ImageData = model.FileBytes,
            UploadedBy = model.UploadedBy,
            UploadedAt = now,
            Status = ReceiptStatus.Pending,
            Type = ReceiptType.Unknown
        };

        _dbContext.Receipts.Add(receipt);
        await _dbContext.SaveChangesAsync(cancellationToken);
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
}
