using System.Security.Claims;
using BookWise.Application.Users;
using BookWise.Domain.Authorization;
using BookWise.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookWise.Api.Users.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminUsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(IUserManagementService userManagementService, ILogger<AdminUsersController> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserListItemResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _userManagementService.GetUsersAsync(cancellationToken);
        return Ok(users.Select(MapUser));
    }

    [HttpPost]
    public async Task<ActionResult<UserListItemResponse>> InviteUser([FromBody] InviteUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var actorId = GetActorUserId();
            var user = await _userManagementService.InviteUserAsync(
                request.FirstName,
                request.LastName,
                request.Role,
                request.Emails,
                actorId,
                cancellationToken);

            return CreatedAtAction(nameof(GetUsers), new { id = user.UserId }, MapUser(user));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpPut("{userId:guid}/role")]
    public async Task<ActionResult<UserListItemResponse>> UpdateRole(Guid userId, [FromBody] UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var actorId = GetActorUserId();
            var user = await _userManagementService.UpdateUserRoleAsync(userId, request.Role, actorId, cancellationToken);
            return Ok(MapUser(user));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpPost("{userId:guid}/emails")]
    public async Task<ActionResult<UserListItemResponse>> AddEmail(Guid userId, [FromBody] AddUserEmailRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var actorId = GetActorUserId();
            await _userManagementService.AddUserEmailAsync(userId, request.Email, actorId, cancellationToken);
            var user = await _userManagementService.GetUserByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return NotFound();
            }

            return Ok(MapUser(user));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpDelete("{userId:guid}/emails/{emailId:guid}")]
    public async Task<IActionResult> RemoveEmail(Guid userId, Guid emailId, CancellationToken cancellationToken)
    {
        try
        {
            var actorId = GetActorUserId();
            await _userManagementService.RemoveUserEmailAsync(userId, emailId, actorId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    private Guid GetActorUserId()
    {
        var userIdClaim = User.FindFirstValue("bookwise:userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var actorId))
        {
            throw new InvalidOperationException("Authenticated user is missing an identifier.");
        }

        return actorId;
    }

    private static UserListItemResponse MapUser(User user) =>
        new(
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Role,
            user.CreatedAt,
            user.CreatedBy,
            user.Emails
                .OrderBy(email => email.Email)
                .Select(e => new UserEmailResponse(e.Id, e.Email, e.CreatedAt, e.CreatedBy))
                .ToArray());
}
