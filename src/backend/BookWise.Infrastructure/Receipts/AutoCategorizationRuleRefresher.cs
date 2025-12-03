using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookWise.Application.Receipts;
using BookWise.Domain.Entities;
using BookWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookWise.Infrastructure.Receipts;

public interface IAutoCategorizationRuleRefresher
{
    Task<int> RefreshRulesAsync(int minOccurrences, CancellationToken cancellationToken);
}

public sealed class AutoCategorizationRuleRefresher : IAutoCategorizationRuleRefresher
{
    private readonly BookWiseDbContext _dbContext;
    private readonly ILogger<AutoCategorizationRuleRefresher> _logger;

    public AutoCategorizationRuleRefresher(BookWiseDbContext dbContext, ILogger<AutoCategorizationRuleRefresher> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> RefreshRulesAsync(int minOccurrences, CancellationToken cancellationToken)
    {
        var aggregates = await _dbContext.ReceiptDecisions
            .AsNoTracking()
            .Where(d =>
                d.Receipt.SellerName != null &&
                d.PurposeAccountId.HasValue &&
                d.PostingAccountId.HasValue)
            .GroupBy(d => new
            {
                SellerName = d.Receipt.SellerName!,
                PurposeAccountId = d.PurposeAccountId!.Value,
                PostingAccountId = d.PostingAccountId!.Value
            })
            .Select(group => new
            {
                group.Key.SellerName,
                group.Key.PurposeAccountId,
                group.Key.PostingAccountId,
                Count = group.Count()
            })
            .Where(x => x.Count >= minOccurrences)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var updated = 0;

        foreach (var aggregate in aggregates)
        {
            var existing = await _dbContext.AccountSuggestionRules
                .SingleOrDefaultAsync(rule =>
                    rule.SellerName == aggregate.SellerName &&
                    rule.PurposeAccountId == aggregate.PurposeAccountId &&
                    rule.PostingAccountId == aggregate.PostingAccountId,
                    cancellationToken);

            if (existing is null)
            {
                _dbContext.AccountSuggestionRules.Add(new AccountSuggestionRule
                {
                    SellerName = aggregate.SellerName,
                    PurposeAccountId = aggregate.PurposeAccountId,
                    PostingAccountId = aggregate.PostingAccountId,
                    OccurrenceCount = aggregate.Count,
                    CreatedAt = now,
                    LastUpdatedAt = now
                });
            }
            else
            {
                existing.OccurrenceCount = aggregate.Count;
                existing.LastUpdatedAt = now;
            }

            updated++;
        }

        if (updated > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated {Count} account suggestion rules (threshold {Threshold}).", updated, minOccurrences);
        }

        return updated;
    }
}
