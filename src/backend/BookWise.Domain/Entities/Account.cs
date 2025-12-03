namespace BookWise.Domain.Entities;

public class Account
{
    public int AccountId { get; set; }
    public string? ExternalAccountNumber { get; set; }
    public string Name { get; set; } = null!;
    public string SegmentCode { get; set; } = null!;
    public int Level { get; set; }
    public AccountType Type { get; set; }

    public int? ParentAccountId { get; set; }
    public Account? ParentAccount { get; set; }
    public ICollection<Account> Children { get; set; } = new List<Account>();
    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
}
