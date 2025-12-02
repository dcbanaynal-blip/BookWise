namespace BookWise.Api.Users.Admin;

public sealed record UserListItemResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt,
    Guid CreatedBy,
    bool IsActive,
    IReadOnlyCollection<UserEmailResponse> Emails);
