namespace BookWise.Domain.Entities;

public class ReceiptLineItem
{
    public int ReceiptLineItemId { get; set; }
    public int ReceiptId { get; set; }
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public string Description { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }

    public Receipt Receipt { get; set; } = null!;
}
