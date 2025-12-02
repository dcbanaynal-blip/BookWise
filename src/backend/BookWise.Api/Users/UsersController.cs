using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BookWise.Domain.Authorization;
using BookWise.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookWise.Api.Users;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly BookWiseDbContext _dbContext;

    public UsersController(BookWiseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue("bookwise:userId") ??
                          User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await _dbContext.Users
            .Include(u => u.Emails)
            .SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
        {
            return Forbid();
        }

        var primaryEmail = user.Emails.FirstOrDefault()?.Email ?? string.Empty;
        var emails = user.Emails.Select(e => e.Email).ToArray();

        return new UserProfileResponse(
            user.UserId,
            user.FirstName,
            user.LastName,
            primaryEmail,
            user.Role,
            user.Role == UserRoles.Admin,
            user.IsActive,
            emails);
    }
}
