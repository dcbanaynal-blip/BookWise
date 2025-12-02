namespace BookWise.Api.Accounts;

public sealed record AccountTreeResponse(
    int AccountId,
    string ExternalAccountNumber,
    string Name,
    string SegmentCode,
    int Level,
    string Type,
    int? ParentAccountId,
    bool HasChildren,
    IReadOnlyCollection<AccountTreeResponse> Children);
