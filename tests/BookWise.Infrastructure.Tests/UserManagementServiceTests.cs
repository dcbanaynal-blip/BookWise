using BookWise.Domain.Entities;
using BookWise.Infrastructure.Persistence;
using BookWise.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BookWise.Infrastructure.Tests;

public class UserManagementServiceTests
{
    [Fact]
    public async Task InviteUserAsync_PersistsUserWithEmails()
    {
        var (service, context) = CreateService();
        var actorId = Guid.NewGuid();

        var user = await service.InviteUserAsync(
            "Jane",
            "Doe",
            role: "Admin",
            new[] { "jane@example.com", "ops@example.com" },
            actorId,
            CancellationToken.None);

        var stored = await context.Users.Include(u => u.Emails).SingleAsync();
        Assert.Equal("Jane", stored.FirstName);
        Assert.Equal("Admin", stored.Role);
        Assert.Equal(2, stored.Emails.Count);
        Assert.Contains(stored.Emails, e => e.Email == "jane@example.com");
        Assert.Contains(stored.Emails, e => e.Email == "ops@example.com");
    }

    [Fact]
    public async Task InviteUserAsync_WhenEmailExists_Throws()
    {
        var (service, context) = CreateService();
        var actorId = Guid.NewGuid();
        context.Users.Add(new User
        {
            UserId = Guid.NewGuid(),
            FirstName = "Existing",
            LastName = "User",
            Role = "Viewer",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorId,
            Emails = new List<UserEmail>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "existing@example.com",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = actorId
                }
            }
        });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InviteUserAsync(
                "Jane",
                "Doe",
                "Accountant",
                new[] { "existing@example.com" },
                actorId,
                CancellationToken.None));
    }

    [Fact]
    public async Task RemoveUserEmailAsync_WhenLastEmail_Throws()
    {
        var (service, context) = CreateService();
        var actorId = Guid.NewGuid();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = "Single",
            LastName = "Email",
            Role = "Viewer",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorId,
            Emails = new List<UserEmail>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "alone@example.com",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = actorId
                }
            }
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RemoveUserEmailAsync(user.UserId, user.Emails.First().Id, actorId, CancellationToken.None));
    }

    private static (UserManagementService service, BookWiseDbContext context) CreateService()
    {
        var options = new DbContextOptionsBuilder<BookWiseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new BookWiseDbContext(options);
        var service = new UserManagementService(context, NullLogger<UserManagementService>.Instance);
        return (service, context);
    }
}
