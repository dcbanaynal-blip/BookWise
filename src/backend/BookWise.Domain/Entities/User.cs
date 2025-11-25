namespace BookWise.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }

    public User CreatedByUser { get; set; } = null!;
    public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    public ICollection<Receipt> UploadedReceipts { get; set; } = new List<Receipt>();
    public ICollection<UserEmail> Emails { get; set; } = new List<UserEmail>();
    public ICollection<UserEmail> UserEmailsCreated { get; set; } = new List<UserEmail>();
    public ICollection<User> InvitedUsers { get; set; } = new List<User>();
}
