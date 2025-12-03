namespace BookWise.Api.Receipts;

public sealed class ReceiptListOptions
{
    public bool UnlinkedOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
