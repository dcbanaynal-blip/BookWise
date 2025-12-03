using System;
using System.Threading;
using System.Threading.Tasks;
using BookWise.Application.Receipts;

namespace BookWise.Infrastructure.Receipts;

public sealed class DatabaseReceiptFileStorage : IReceiptFileStorage
{
    public Task<ReceiptFileSaveResult> SaveAsync(ReceiptFilePayload payload, CancellationToken cancellationToken)
    {
        if (payload.Data.Length == 0)
        {
            throw new InvalidOperationException("Receipt payload cannot be empty.");
        }

        var contentType = string.IsNullOrWhiteSpace(payload.ContentType)
            ? "application/octet-stream"
            : payload.ContentType;

        return Task.FromResult(new ReceiptFileSaveResult(payload.Data, contentType));
    }
}
