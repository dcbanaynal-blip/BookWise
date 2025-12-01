namespace BookWise.Api.Users.Admin;

public sealed record UserEmailResponse(Guid Id, string Email, DateTime CreatedAt, Guid CreatedBy);
