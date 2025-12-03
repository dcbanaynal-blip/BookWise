namespace BookWise.Domain.Entities;

public class AccountSuggestionRule
{
    public int AccountSuggestionRuleId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int PurposeAccountId { get; set; }
    public int PostingAccountId { get; set; }
    public int OccurrenceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
