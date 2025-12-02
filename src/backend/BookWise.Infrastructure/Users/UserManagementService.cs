using BookWise.Application.Users;
using BookWise.Domain.Authorization;
using BookWise.Domain.Entities;
using BookWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookWise.Infrastructure.Users;

public class UserManagementService : IUserManagementService
{
    private readonly BookWiseDbContext _dbContext;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(BookWiseDbContext dbContext, ILogger<UserManagementService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<User>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Include(u => u.Emails)
            .Include(u => u.CreatedByUser)
            .AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Include(u => u.Emails)
            .Include(u => u.CreatedByUser)
            .SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);
    }

    public async Task<User> InviteUserAsync(
        string firstName,
        string lastName,
        string role,
        IEnumerable<string> emails,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateRole(role);
        var normalizedEmails = NormalizeEmails(emails).ToArray();
        await EnsureEmailsAreUnique(normalizedEmails, cancellationToken);

        var now = DateTime.UtcNow;
        var newUserId = Guid.NewGuid();

        var user = new User
        {
            UserId = newUserId,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            CreatedAt = now,
            CreatedBy = actorUserId,
            UpdatedAt = now,
            UpdatedBy = actorUserId
            ,
            IsActive = true
        };

        foreach (var email in normalizedEmails)
        {
            user.Emails.Add(new UserEmail
            {
                Id = Guid.NewGuid(),
                UserId = newUserId,
                Email = email,
                CreatedAt = now,
                CreatedBy = actorUserId
            });
        }

        _dbContext.Users.Add(user);
        await SaveChangesGuardingEmailConflicts(cancellationToken);

        _logger.LogInformation("User {Email} invited by {ActorId}", normalizedEmails.FirstOrDefault(), actorUserId);
        return user;
    }

    public async Task<User> UpdateUserRoleAsync(Guid userId, string role, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ValidateRole(role);

        var user = await _dbContext.Users.FindAsync([userId], cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = actorUserId;
        await SaveChangesGuardingEmailConflicts(cancellationToken);
        await _dbContext.Entry(user).Collection(u => u.Emails).LoadAsync(cancellationToken);

        _logger.LogInformation("User {UserId} role updated to {Role} by {ActorId}", userId, role, actorUserId);
        return user;
    }

    public async Task<UserEmail> AddUserEmailAsync(Guid userId, string email, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        await EnsureEmailsAreUnique([normalizedEmail], cancellationToken);

        var now = DateTime.UtcNow;

        var userExists = await _dbContext.Users.AnyAsync(u => u.UserId == userId, cancellationToken);

        if (!userExists)
        {
            throw new InvalidOperationException("User not found.");
        }

        var userEmail = new UserEmail
        {
            UserId = userId,
            Email = normalizedEmail,
            CreatedAt = now,
            CreatedBy = actorUserId
        };

        _dbContext.UserEmails.Add(userEmail);
        await SaveChangesGuardingEmailConflicts(cancellationToken);

        _logger.LogInformation("Email {Email} added to user {UserId} by {ActorId}", normalizedEmail, userId, actorUserId);
        return userEmail;
    }

    public async Task RemoveUserEmailAsync(Guid userId, Guid emailId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Emails)
            .SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var userEmail = user.Emails.SingleOrDefault(e => e.Id == emailId);
        if (userEmail is null)
        {
            throw new InvalidOperationException("Email record not found.");
        }

        if (user.Emails.Count <= 1)
        {
            throw new InvalidOperationException("Users must have at least one email address.");
        }

        _dbContext.UserEmails.Remove(userEmail);
        await SaveChangesGuardingEmailConflicts(cancellationToken);

        _logger.LogInformation("Email {EmailId} removed from user {UserId} by {ActorId}", emailId, userId, actorUserId);
    }

    public async Task<User> UpdateUserStatusAsync(Guid userId, bool isActive, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync([userId], cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = actorUserId;

        await SaveChangesGuardingEmailConflicts(cancellationToken);
        await _dbContext.Entry(user).Collection(u => u.Emails).LoadAsync(cancellationToken);

        _logger.LogInformation("User {UserId} status set to {Status} by {ActorId}", userId, isActive, actorUserId);
        return user;
    }

    public async Task<User> UpdateUserDetailsAsync(Guid userId, string firstName, string lastName, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new InvalidOperationException("First name and last name are required.");
        }

        var user = await _dbContext.Users.FindAsync([userId], cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = actorUserId;

        await SaveChangesGuardingEmailConflicts(cancellationToken);
        await _dbContext.Entry(user).Collection(u => u.Emails).LoadAsync(cancellationToken);

        _logger.LogInformation("User {UserId} name updated by {ActorId}", userId, actorUserId);
        return user;
    }

    private static void ValidateRole(string role)
    {
        if (!UserRoles.All.Contains(role))
        {
            throw new InvalidOperationException("Unknown user role.");
        }
    }

    private async Task EnsureEmailsAreUnique(IEnumerable<string> emails, CancellationToken cancellationToken)
    {
        var emailSet = new HashSet<string>(emails.Select(NormalizeEmail), StringComparer.OrdinalIgnoreCase);
        var duplicates = await _dbContext.UserEmails
            .Where(ue => emailSet.Contains(ue.Email))
            .Select(ue => ue.Email)
            .ToListAsync(cancellationToken);

        if (duplicates.Count > 0)
        {
            throw new InvalidOperationException($"Email address already registered: {duplicates.First()}");
        }
    }

    private static IEnumerable<string> NormalizeEmails(IEnumerable<string> emails) =>
        emails.Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(NormalizeEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private async Task SaveChangesGuardingEmailConflicts(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict detected while updating user emails.");
            throw new InvalidOperationException(
                "The user was modified or deleted by another operation. Refresh and try again.", ex);
        }
        catch (DbUpdateException ex) when (IsUniqueEmailConstraint(ex))
        {
            _logger.LogWarning(ex, "Duplicate email detected while persisting user changes.");
            throw new InvalidOperationException("Email address already registered.", ex);
        }
    }

    private static bool IsUniqueEmailConstraint(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("UserEmails", StringComparison.OrdinalIgnoreCase)
            && message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }
}
