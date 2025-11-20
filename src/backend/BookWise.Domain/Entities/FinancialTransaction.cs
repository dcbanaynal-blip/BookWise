namespace BookWise.Domain.Entities;

public class FinancialTransaction
{
    public int TransactionId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Creator { get; set; } = null!;
    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    public Receipt? Receipt { get; set; }
}
