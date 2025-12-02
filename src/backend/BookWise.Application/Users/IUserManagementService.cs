using BookWise.Domain.Entities;

namespace BookWise.Application.Users;

public interface IUserManagementService
{
    Task<IReadOnlyList<User>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User> InviteUserAsync(
        string firstName,
        string lastName,
        string role,
        IEnumerable<string> emails,
        Guid actorUserId,
        CancellationToken cancellationToken = default);
    Task<User> UpdateUserRoleAsync(Guid userId, string role, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<User> UpdateUserDetailsAsync(Guid userId, string firstName, string lastName, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<User> UpdateUserStatusAsync(Guid userId, bool isActive, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<UserEmail> AddUserEmailAsync(Guid userId, string email, Guid actorUserId, CancellationToken cancellationToken = default);
    Task RemoveUserEmailAsync(Guid userId, Guid emailId, Guid actorUserId, CancellationToken cancellationToken = default);
}
