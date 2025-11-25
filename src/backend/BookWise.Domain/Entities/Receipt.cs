namespace BookWise.Domain.Entities;

public class Receipt
{
    public int ReceiptId { get; set; }
    public int? TransactionId { get; set; }
    public FinancialTransaction? Transaction { get; set; }
    public ReceiptType Type { get; set; } = ReceiptType.Unknown;
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string? MimeType { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? DocumentDate { get; set; }
    public string? SellerName { get; set; }
    public string? SellerTaxId { get; set; }
    public string? SellerAddress { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerTaxId { get; set; }
    public string? CustomerAddress { get; set; }
    public string? Terms { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? BusinessStyle { get; set; }
    public string? Notes { get; set; }
    public decimal? SubtotalAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? WithholdingTaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? OcrText { get; set; }
    public ReceiptStatus Status { get; set; }

    public User Uploader { get; set; } = null!;
    public ICollection<ReceiptLineItem> LineItems { get; set; } = new List<ReceiptLineItem>();
}
