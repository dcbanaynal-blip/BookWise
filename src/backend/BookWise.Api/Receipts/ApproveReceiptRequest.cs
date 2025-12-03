namespace BookWise.Api.Receipts;

public sealed class ApproveReceiptRequest
{
    public int? PurposeAccountId { get; set; }
    public int? PostingAccountId { get; set; }
    public bool? VatOverride { get; set; }
    public decimal? TotalOverride { get; set; }
    public string? Notes { get; set; }
}
