namespace BookWise.Domain.Entities;

public class UserEmail
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }

    public User User { get; set; } = null!;
    public User Creator { get; set; } = null!;
}
