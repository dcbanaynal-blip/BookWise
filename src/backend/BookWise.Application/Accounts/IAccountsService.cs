using BookWise.Domain.Entities;

namespace BookWise.Application.Accounts;

public interface IAccountsService
{
    Task<IReadOnlyList<Account>> GetAccountsAsync(CancellationToken cancellationToken = default);

    Task<Account?> GetAccountByIdAsync(int accountId, CancellationToken cancellationToken = default);

    Task<Account> CreateAccountAsync(CreateAccountModel request, CancellationToken cancellationToken = default);

    Task<Account> UpdateAccountAsync(int accountId, UpdateAccountModel request, CancellationToken cancellationToken = default);

    Task DeleteAccountAsync(int accountId, CancellationToken cancellationToken = default);
}

public sealed record CreateAccountModel(
    string? ExternalAccountNumber,
    string Name,
    string SegmentCode,
    AccountType Type,
    int? ParentAccountId);

public sealed record UpdateAccountModel(
    string Name,
    string SegmentCode,
    AccountType Type);
