using System;
using System.Collections.Generic;

namespace BookWise.Api.Users;

public sealed record UserProfileResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsAdmin,
    IReadOnlyCollection<string> Emails);
