namespace BookWise.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public byte[]? PasswordHash { get; set; }
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    public ICollection<Receipt> UploadedReceipts { get; set; } = new List<Receipt>();
}
