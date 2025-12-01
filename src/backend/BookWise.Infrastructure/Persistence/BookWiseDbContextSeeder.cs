using System;
using System.Threading;
using System.Threading.Tasks;
using BookWise.Domain.Authorization;
using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookWise.Infrastructure.Persistence;

public static class BookWiseDbContextSeeder
{
    private const string DefaultAdminEmail = "dcbanaynal@fadi.com.ph";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookWiseDbContext>();

        await context.Database.MigrateAsync(cancellationToken);

        var normalizedEmail = NormalizeEmail(DefaultAdminEmail);
        var userEmail = await context.UserEmails
            .Include(ue => ue.User)
            .SingleOrDefaultAsync(ue => ue.Email == normalizedEmail, cancellationToken);

        if (userEmail is null)
        {
            var adminUserId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var adminUser = new User
            {
                UserId = adminUserId,
                FirstName = "DC",
                LastName = "Banaynal",
                Role = UserRoles.Admin,
                CreatedAt = now,
                CreatedBy = adminUserId
            };

            var allowlistedEmail = new UserEmail
            {
                Id = Guid.NewGuid(),
                UserId = adminUserId,
                Email = normalizedEmail,
                CreatedAt = now,
                CreatedBy = adminUserId
            };

            adminUser.Emails.Add(allowlistedEmail);

            context.Users.Add(adminUser);
        }
        else if (!string.Equals(userEmail.User.Role, UserRoles.Admin, StringComparison.Ordinal))
        {
            userEmail.User.Role = UserRoles.Admin;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
