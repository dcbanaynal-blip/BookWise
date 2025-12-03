using System;
using System.Collections.Generic;

namespace BookWise.Api.Receipts;

/// <summary>
/// Metadata supplied together with the uploaded binary payload.
/// The image itself will be transmitted in raw binary form (multipart file) once the upload endpoint is implemented.
/// </summary>
public sealed class ReceiptUploadRequest
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
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

public sealed record ReceiptUploadResponse(
    int ReceiptId,
    string Status,
    DateTime UploadedAt);

public sealed record ReceiptListItemResponse(
    int ReceiptId,
    string? SellerName,
    DateTime? DocumentDate,
    decimal? TotalAmount,
    string Status,
    bool? IsVatApplicable,
    DateTime UploadedAt);

public sealed record ReceiptDetailResponse(
    int ReceiptId,
    string? SellerName,
    string? SellerTaxId,
    string? SellerAddress,
    string? CustomerName,
    string? CustomerTaxId,
    string? CustomerAddress,
    DateTime? DocumentDate,
    decimal? SubtotalAmount,
    decimal? TaxAmount,
    decimal? DiscountAmount,
    decimal? WithholdingTaxAmount,
    decimal? TotalAmount,
    bool? IsVatApplicable,
    string Status,
    string? OcrText,
    IReadOnlyCollection<ReceiptLineItemResponse> LineItems,
    IReadOnlyCollection<ReceiptDecisionResponse> Decisions);

public sealed record ReceiptLineItemResponse(
    int ReceiptLineItemId,
    decimal Quantity,
    string? Unit,
    string Description,
    decimal UnitPrice,
    decimal Amount);

public sealed record ReceiptDecisionResponse(
    int ReceiptDecisionId,
    int? PurposeAccountId,
    int? PostingAccountId,
    bool? VatOverride,
    decimal? TotalOverride,
    string? Notes,
    DateTime CreatedAt,
    Guid CreatedBy);
