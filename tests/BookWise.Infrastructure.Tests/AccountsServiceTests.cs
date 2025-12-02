using BookWise.Application.Accounts;
using BookWise.Domain.Entities;
using BookWise.Infrastructure.Accounts;
using BookWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BookWise.Infrastructure.Tests;

public class AccountsServiceTests
{
    [Fact]
    public async Task CreateAccountAsync_WithParent_ComputesLevel()
    {
        var (service, context) = CreateService();
        var parent = new Account
        {
            ExternalAccountNumber = "1000",
            Name = "Assets",
            SegmentCode = "1000",
            Level = 1,
            Type = AccountType.Asset
        };
        context.Accounts.Add(parent);
        await context.SaveChangesAsync();

        var created = await service.CreateAccountAsync(
            new CreateAccountModel("1000-200", "Cash", "200", AccountType.Asset, parent.AccountId),
            CancellationToken.None);

        Assert.Equal(parent.Level + 1, created.Level);
        Assert.Equal(parent.AccountId, created.ParentAccountId);
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenHasChildren_Throws()
    {
        var (service, context) = CreateService();
        var parent = new Account
        {
            ExternalAccountNumber = "4000",
            Name = "Revenue",
            SegmentCode = "4000",
            Level = 1,
            Type = AccountType.Revenue
        };
        var child = new Account
        {
            ExternalAccountNumber = "4000-100",
            Name = "Product Revenue",
            SegmentCode = "100",
            Level = 2,
            ParentAccount = parent,
            Type = AccountType.Revenue
        };
        context.Accounts.AddRange(parent, child);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteAccountAsync(parent.AccountId, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAccountAsync_UpdatesFields()
    {
        var (service, context) = CreateService();
        var account = new Account
        {
            ExternalAccountNumber = "5000",
            Name = "Expenses",
            SegmentCode = "5000",
            Level = 1,
            Type = AccountType.Expense
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var updated = await service.UpdateAccountAsync(
            account.AccountId,
            new UpdateAccountModel("COGS", "5000A", AccountType.Expense),
            CancellationToken.None);

        Assert.Equal("COGS", updated.Name);
        Assert.Equal("5000A", updated.SegmentCode);
    }

    private static (AccountsService service, BookWiseDbContext context) CreateService()
    {
        var options = new DbContextOptionsBuilder<BookWiseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new BookWiseDbContext(options);
        var service = new AccountsService(context, NullLogger<AccountsService>.Instance);
        return (service, context);
    }
}
