namespace BookWise.Domain.Entities;

public class Entry
{
    public int EntryId { get; set; }
    public int TransactionId { get; set; }
    public FinancialTransaction Transaction { get; set; } = null!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
