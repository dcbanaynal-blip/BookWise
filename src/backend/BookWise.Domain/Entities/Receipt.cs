namespace BookWise.Domain.Entities;

public class Receipt
{
    public int ReceiptId { get; set; }
    public int? TransactionId { get; set; }
    public FinancialTransaction? Transaction { get; set; }
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string? MimeType { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? OcrText { get; set; }
    public ReceiptStatus Status { get; set; }

    public User Uploader { get; set; } = null!;
}
