using BookWise.Application.Accounts;
using BookWise.Domain.Entities;
using BookWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookWise.Infrastructure.Accounts;

public sealed class AccountsService : IAccountsService
{
    private readonly BookWiseDbContext _dbContext;
    private readonly ILogger<AccountsService> _logger;

    public AccountsService(BookWiseDbContext dbContext, ILogger<AccountsService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Account>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .OrderBy(a => a.Level)
            .ThenBy(a => a.SegmentCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<Account?> GetAccountByIdAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .Include(a => a.Children)
            .SingleOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
    }

    public async Task<Account> CreateAccountAsync(CreateAccountModel request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var normalizedExternal = Normalize(request.ExternalAccountNumber);
        var normalizedName = Normalize(request.Name);
        var normalizedSegment = Normalize(request.SegmentCode);

        if (string.IsNullOrWhiteSpace(normalizedExternal))
        {
            throw new InvalidOperationException("External account number is required.");
        }

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new InvalidOperationException("Account name is required.");
        }

        if (string.IsNullOrWhiteSpace(normalizedSegment))
        {
            throw new InvalidOperationException("Segment code is required.");
        }

        var account = new Account
        {
            ExternalAccountNumber = normalizedExternal,
            Name = normalizedName,
            SegmentCode = normalizedSegment
        };

        await ApplyParentMetadataAsync(account, request.ParentAccountId, request.Type, cancellationToken);

        _dbContext.Accounts.Add(account);
        await SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Account {ExternalAccountNumber} created with id {AccountId}", account.ExternalAccountNumber, account.AccountId);
        return account;
    }

    public async Task<Account> UpdateAccountAsync(int accountId, UpdateAccountModel request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var account = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
        if (account is null)
        {
            throw new InvalidOperationException("Account not found.");
        }

        var normalizedName = Normalize(request.Name);
        var normalizedSegment = Normalize(request.SegmentCode);

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new InvalidOperationException("Account name is required.");
        }

        if (string.IsNullOrWhiteSpace(normalizedSegment))
        {
            throw new InvalidOperationException("Segment code is required.");
        }

        account.Name = normalizedName;
        account.SegmentCode = normalizedSegment;

        await ApplyAccountTypeForUpdateAsync(account, request.Type, cancellationToken);

        await SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Account {AccountId} updated", accountId);
        return account;
    }

    public async Task DeleteAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
        if (account is null)
        {
            throw new InvalidOperationException("Account not found.");
        }

        var hasChildren = await _dbContext.Accounts
            .AsNoTracking()
            .AnyAsync(a => a.ParentAccountId == accountId, cancellationToken);
        if (hasChildren)
        {
            throw new InvalidOperationException("Cannot delete an account that has child accounts.");
        }

        var hasEntries = await _dbContext.Entries
            .AsNoTracking()
            .AnyAsync(e => e.AccountId == accountId, cancellationToken);
        if (hasEntries)
        {
            throw new InvalidOperationException("Cannot delete an account that has ledger entries.");
        }

        _dbContext.Accounts.Remove(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Account {AccountId} deleted", accountId);
    }

    private async Task ApplyParentMetadataAsync(Account account, int? parentAccountId, AccountType requestedType, CancellationToken cancellationToken)
    {
        if (parentAccountId is null)
        {
            account.Level = 1;
            account.ParentAccountId = null;
            account.Type = requestedType;
            return;
        }

        var parent = await _dbContext.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.AccountId == parentAccountId.Value, cancellationToken);

        if (parent is null)
        {
            throw new InvalidOperationException("Parent account not found.");
        }

        account.ParentAccountId = parent.AccountId;
        account.Level = parent.Level + 1;
        account.Type = parent.Type;
    }

    private async Task ApplyAccountTypeForUpdateAsync(Account account, AccountType requestedType, CancellationToken cancellationToken)
    {
        if (account.ParentAccountId is null)
        {
            account.Type = requestedType;
            return;
        }

        var parentType = await _dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.AccountId == account.ParentAccountId.Value)
            .Select(a => a.Type)
            .SingleOrDefaultAsync(cancellationToken);

        if (parentType == null)
        {
            throw new InvalidOperationException("Parent account not found.");
        }

        account.Type = parentType;
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (TryTranslateUniqueConstraint(ex, out var message))
        {
            _logger.LogWarning(ex, "Constraint violation while saving account.");
            throw new InvalidOperationException(message, ex);
        }
    }

    private static bool TryTranslateUniqueConstraint(DbUpdateException ex, out string message)
    {
        var error = ex.InnerException?.Message ?? ex.Message ?? string.Empty;

        if (error.Contains("IX_Accounts_ExternalAccountNumber", StringComparison.OrdinalIgnoreCase))
        {
            message = "An account with the same external account number already exists.";
            return true;
        }

        if (error.Contains("IX_Accounts_RootSegmentCode", StringComparison.OrdinalIgnoreCase))
        {
            message = "Segment code must be unique among root-level accounts.";
            return true;
        }

        if (error.Contains("IX_Accounts_ParentSegmentCode", StringComparison.OrdinalIgnoreCase))
        {
            message = "Segment code must be unique within the selected parent account.";
            return true;
        }

        message = string.Empty;
        return false;
    }

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;
}
