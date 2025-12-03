namespace BookWise.Domain.Entities;

public class ReceiptDecision
{
    public int ReceiptDecisionId { get; set; }
    public int ReceiptId { get; set; }
    public int? PurposeAccountId { get; set; }
    public int? PostingAccountId { get; set; }
    public bool? VatOverride { get; set; }
    public decimal? TotalOverride { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Receipt Receipt { get; set; } = null!;
}
