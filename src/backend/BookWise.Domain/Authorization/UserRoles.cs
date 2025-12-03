using System.Collections.Generic;

namespace BookWise.Domain.Authorization;

/// <summary>
/// Application user roles used for authorization and feature gating.
/// </summary>
public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Accountant = "Accountant";
    public const string Bookkeeper = "Bookkeeper";
    public const string Viewer = "Viewer";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        Admin,
        Accountant,
        Bookkeeper,
        Viewer
    };
}
