using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookWise.Domain.Entities;

namespace BookWise.Application.Receipts;

public interface IReceiptsService
{
    Task<Receipt> CreateReceiptAsync(CreateReceiptModel model, CancellationToken cancellationToken);
    Task<IReadOnlyList<Receipt>> GetReceiptsAsync(int page, int pageSize, bool unlinkedOnly, CancellationToken cancellationToken);
    Task<Receipt?> GetReceiptByIdAsync(int receiptId, CancellationToken cancellationToken);
    Task<Receipt> ApproveReceiptAsync(int receiptId, ApproveReceiptModel model, CancellationToken cancellationToken);
}

public sealed class CreateReceiptModel
{
    public required Guid UploadedBy { get; init; }
    public required string OriginalFileName { get; init; }
    public required string ContentType { get; init; }
    public required long FileSize { get; init; }
    public required byte[] FileBytes { get; init; }
    public DateTime? DocumentDate { get; init; }
    public string? SellerName { get; init; }
    public string? SellerTaxId { get; init; }
    public string? SellerAddress { get; init; }
    public string? CustomerName { get; init; }
    public string? CustomerTaxId { get; init; }
    public string? CustomerAddress { get; init; }
    public string? Notes { get; init; }
    public bool? IsVatApplicable { get; init; }
}

public sealed class ApproveReceiptModel
{
    public required Guid ActorUserId { get; init; }
    public int? PurposeAccountId { get; init; }
    public int? PostingAccountId { get; init; }
    public bool? VatOverride { get; init; }
    public decimal? TotalOverride { get; init; }
    public string? Notes { get; init; }
}
