using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BookWise.Application.Receipts;
using BookWise.Domain.Authorization;
using BookWise.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookWise.Api.Receipts;

[ApiController]
[Route("api/receipts")]
[Authorize]
public sealed class ReceiptsController : ControllerBase
{
    private readonly IReceiptsService _receiptsService;

    public ReceiptsController(IReceiptsService receiptsService)
    {
        _receiptsService = receiptsService;
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Accountant},{UserRoles.Bookkeeper}")]
    [RequestSizeLimit(25_000_000)] // ~25 MB
    public async Task<ActionResult<ReceiptUploadResponse>> UploadReceipt(
        [FromForm] ReceiptUploadRequest payload,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A non-empty receipt image is required.");
        }

        if (file.Length > 25_000_000)
        {
            return BadRequest("Receipt file size exceeds 25MB limit.");
        }

        if (!ReceiptUploadRequestValidator.IsSupportedMime(file.ContentType))
        {
            return BadRequest("Unsupported file type. Please upload JPEG, PNG, or PDF receipts.");
        }

        var uploaderId = GetCurrentUserId();
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        var createModel = new CreateReceiptModel
        {
            UploadedBy = uploaderId,
            OriginalFileName = string.IsNullOrWhiteSpace(payload.FileName) ? file.FileName : payload.FileName,
            ContentType = !string.IsNullOrWhiteSpace(payload.ContentType) ? payload.ContentType : file.ContentType ?? "application/octet-stream",
            FileSize = file.Length,
            FileBytes = stream.ToArray(),
            DocumentDate = payload.DocumentDate,
            SellerName = payload.SellerName,
            SellerTaxId = payload.SellerTaxId,
            SellerAddress = payload.SellerAddress,
            CustomerName = payload.CustomerName,
            CustomerTaxId = payload.CustomerTaxId,
            CustomerAddress = payload.CustomerAddress,
            Notes = payload.Notes,
            IsVatApplicable = payload.IsVatApplicable
        };

        var receipt = await _receiptsService.CreateReceiptAsync(createModel, cancellationToken);
        var response = new ReceiptUploadResponse(receipt.ReceiptId, receipt.Status.ToString(), receipt.UploadedAt);
        return CreatedAtAction(nameof(GetReceiptById), new { receiptId = receipt.ReceiptId }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ReceiptListItemResponse>>> GetReceipts([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var receipts = await _receiptsService.GetReceiptsAsync(page, pageSize, cancellationToken);
        var response = receipts
            .Select(r => new ReceiptListItemResponse(
                r.ReceiptId,
                r.SellerName,
                r.DocumentDate,
                r.TotalAmount,
                r.Status.ToString(),
                r.IsVatApplicable,
                r.UploadedAt))
            .ToArray();

        return Ok(response);
    }

    [HttpGet("{receiptId:int}")]
    public async Task<ActionResult<ReceiptDetailResponse>> GetReceiptById(int receiptId, CancellationToken cancellationToken)
    {
        var receipt = await _receiptsService.GetReceiptByIdAsync(receiptId, cancellationToken);
        if (receipt is null)
        {
            return NotFound();
        }

        return Ok(MapDetail(receipt));
    }

    [HttpPost("{receiptId:int}/approve")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Accountant}")]
    public async Task<ActionResult<ReceiptDetailResponse>> ApproveReceipt(int receiptId, [FromBody] ApproveReceiptRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var actorId = GetCurrentUserId();
            await _receiptsService.ApproveReceiptAsync(receiptId, new ApproveReceiptModel
            {
                ActorUserId = actorId,
                PurposeAccountId = request.PurposeAccountId,
                PostingAccountId = request.PostingAccountId,
                VatOverride = request.VatOverride,
                TotalOverride = request.TotalOverride,
                Notes = request.Notes
            }, cancellationToken);
            var refreshed = await _receiptsService.GetReceiptByIdAsync(receiptId, cancellationToken);
            if (refreshed is null)
            {
                return NotFound();
            }

            return Ok(MapDetail(refreshed));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue("bookwise:userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new InvalidOperationException("Authenticated user is missing identifier claim.");
        }

        return userId;
    }

    private static ReceiptDetailResponse MapDetail(Receipt receipt) =>
        new(
            receipt.ReceiptId,
            receipt.SellerName,
            receipt.SellerTaxId,
            receipt.SellerAddress,
            receipt.CustomerName,
            receipt.CustomerTaxId,
            receipt.CustomerAddress,
            receipt.DocumentDate,
            receipt.SubtotalAmount,
            receipt.TaxAmount,
            receipt.DiscountAmount,
            receipt.WithholdingTaxAmount,
            receipt.TotalAmount,
            receipt.IsVatApplicable,
            receipt.Status.ToString(),
            receipt.OcrText,
            receipt.LineItems.Select(item =>
                new ReceiptLineItemResponse(
                    item.ReceiptLineItemId,
                    item.Quantity,
                    item.Unit,
                    item.Description,
                    item.UnitPrice,
                    item.Amount)).ToArray(),
            receipt.Decisions?.Select(decision => new ReceiptDecisionResponse(
                decision.ReceiptDecisionId,
                decision.PurposeAccountId,
                decision.PostingAccountId,
                decision.VatOverride,
                decision.TotalOverride,
                decision.Notes,
                decision.CreatedAt,
                decision.CreatedBy)).ToArray() ?? Array.Empty<ReceiptDecisionResponse>());
}
