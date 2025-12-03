using System;
using BookWise.Application.Accounts;
using BookWise.Domain.Authorization;
using BookWise.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookWise.Api.Accounts;

[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountsService _accountsService;

    public AccountsController(IAccountsService accountsService)
    {
        _accountsService = accountsService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AccountResponse>>> GetAccounts([FromQuery] bool includeTree, [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var accounts = await _accountsService.GetAccountsAsync(cancellationToken);

        if (includeTree && !string.IsNullOrWhiteSpace(search))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Tree view and search cannot be combined.");
        }

        var filtered = FilterAccounts(accounts, search);

        var fullSegmentMap = BuildFullSegmentMap(filtered);

        if (includeTree)
        {
            var tree = BuildTree(filtered, fullSegmentMap);
            return Ok(tree);
        }

        var hasChildrenMap = BuildHasChildrenMap(filtered);
        var response = filtered
            .Select(account => MapAccount(
                account,
                hasChildrenMap.TryGetValue(account.AccountId, out var hasChildren) && hasChildren,
                fullSegmentMap.GetValueOrDefault(account.AccountId, account.SegmentCode)))
            .ToArray();

        return Ok(response);
    }

    [HttpGet("{accountId:int}")]
    public async Task<ActionResult<AccountResponse>> GetAccount(int accountId, CancellationToken cancellationToken)
    {
        var account = await _accountsService.GetAccountByIdAsync(accountId, cancellationToken);
        if (account is null)
        {
            return NotFound();
        }

        var fullSegment = BuildFullSegmentForAccount(account);
        var hasChildren = account.Children?.Count > 0;
        return Ok(MapAccount(account, hasChildren, fullSegment));
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Accountant}")]
    public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var account = await _accountsService.CreateAccountAsync(
                new CreateAccountModel(
                    request.ExternalAccountNumber,
                    request.Name,
                    request.SegmentCode,
                    request.Type,
                    request.ParentAccountId),
                cancellationToken);

            var hydrated = await _accountsService.GetAccountByIdAsync(account.AccountId, cancellationToken) ?? account;
            var fullSegment = hydrated.ParentAccount is null
                ? hydrated.SegmentCode
                : BuildFullSegmentForAccount(hydrated);

            var response = MapAccount(hydrated, hydrated.Children?.Count > 0, fullSegment);
            return CreatedAtAction(nameof(GetAccount), new { accountId = account.AccountId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpPut("{accountId:int}")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Accountant}")]
    public async Task<ActionResult<AccountResponse>> UpdateAccount(int accountId, [FromBody] UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            await _accountsService.UpdateAccountAsync(
                accountId,
                new UpdateAccountModel(request.Name, request.SegmentCode, request.Type),
                cancellationToken);

            var updatedAccount = await _accountsService.GetAccountByIdAsync(accountId, cancellationToken);
            if (updatedAccount is null)
            {
                return NotFound();
            }

            var fullSegment = BuildFullSegmentForAccount(updatedAccount);
            return Ok(MapAccount(updatedAccount, updatedAccount.Children?.Count > 0, fullSegment));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpDelete("{accountId:int}")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Accountant}")]
    public async Task<IActionResult> DeleteAccount(int accountId, CancellationToken cancellationToken)
    {
        try
        {
            await _accountsService.DeleteAccountAsync(accountId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    private static AccountResponse MapAccount(Account account, bool hasChildren, string fullSegment) =>
        new(
            account.AccountId,
            account.ExternalAccountNumber,
            account.Name,
            account.SegmentCode,
            fullSegment,
            account.Level,
            account.Type.ToString(),
            account.ParentAccountId,
            hasChildren);

    private static IReadOnlyCollection<AccountTreeResponse> BuildTree(IEnumerable<Account> accounts, IReadOnlyDictionary<int, string> fullSegmentMap)
    {
        var lookup = accounts.ToLookup(a => a.ParentAccountId);

        IReadOnlyList<AccountTreeResponse> Build(int? parentId)
        {
            return lookup[parentId]
                .OrderBy(a => a.Level)
                .ThenBy(a => a.SegmentCode)
                .Select(a =>
                    new AccountTreeResponse(
                        a.AccountId,
                        a.ExternalAccountNumber,
                        a.Name,
                        a.SegmentCode,
                        fullSegmentMap.GetValueOrDefault(a.AccountId, a.SegmentCode),
                        a.Level,
                        a.Type.ToString(),
                        a.ParentAccountId,
                        lookup[a.AccountId].Any(),
                        Build(a.AccountId)))
                .ToList();
        }

        return Build(null);
    }

    private static Dictionary<int, bool> BuildHasChildrenMap(IEnumerable<Account> accounts)
    {
        return accounts
            .Where(a => a.ParentAccountId.HasValue)
            .GroupBy(a => a.ParentAccountId!.Value)
            .ToDictionary(group => group.Key, _ => true);
    }

    private static IReadOnlyList<Account> FilterAccounts(IReadOnlyList<Account> accounts, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return accounts;
        }

        var term = search.Trim();
        return accounts
            .Where(a =>
                a.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                a.ExternalAccountNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                a.SegmentCode.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static IReadOnlyDictionary<int, string> BuildFullSegmentMap(IEnumerable<Account> accounts)
    {
        var lookup = accounts.ToLookup(a => a.ParentAccountId);
        var result = new Dictionary<int, string>();

        void Assign(Account account, string? parentSegment)
        {
            var fullSegment = string.IsNullOrEmpty(parentSegment)
                ? account.SegmentCode
                : $"{parentSegment}-{account.SegmentCode}";

            result[account.AccountId] = fullSegment;

            foreach (var child in lookup[account.AccountId])
            {
                Assign(child, fullSegment);
            }
        }

        foreach (var root in lookup[null])
        {
            Assign(root, null);
        }

        return result;
    }

    private static string BuildFullSegmentForAccount(Account account)
    {
        var segments = new List<string>();
        var current = account;

        while (current is not null)
        {
            segments.Insert(0, current.SegmentCode);
            current = current.ParentAccount;
        }

        return string.Join('-', segments);
    }
}
