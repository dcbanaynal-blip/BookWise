using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookWise.Infrastructure.Persistence;

public class BookWiseDbContext(DbContextOptions<BookWiseDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserEmail> UserEmails => Set<UserEmail>();
    public DbSet<FinancialTransaction> Transactions => Set<FinancialTransaction>();
    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReceiptLineItem> ReceiptLineItems => Set<ReceiptLineItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookWiseDbContext).Assembly);
    }
}
