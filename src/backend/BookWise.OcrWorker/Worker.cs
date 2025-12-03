using BookWise.Infrastructure.Ocr;
using BookWise.Infrastructure.Receipts;
using Microsoft.Extensions.Options;

namespace BookWise.OcrWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<OcrWorkerOptions> _options;
    private DateTime _lastRuleRefreshUtc = DateTime.MinValue;
    private DateTime _lastBacklogSnapshotUtc = DateTime.MinValue;

    public Worker(
        ILogger<Worker> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<OcrWorkerOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var pipeline = scope.ServiceProvider.GetRequiredService<IReceiptOcrPipeline>();
            var ruleRefresher = scope.ServiceProvider.GetRequiredService<IAutoCategorizationRuleRefresher>();

            try
            {
                var normalized = await pipeline.PreprocessPendingReceiptsAsync(
                    _options.Value.BatchSize,
                    stoppingToken);

                var extracted = await pipeline.ExtractReceiptContentAsync(
                    _options.Value.BatchSize,
                    stoppingToken);

                if ((DateTime.UtcNow - _lastRuleRefreshUtc).TotalMinutes >= _options.Value.RuleRefreshMinutes)
                {
                    await ruleRefresher.RefreshRulesAsync(_options.Value.RuleMinOccurrences, stoppingToken);
                    _lastRuleRefreshUtc = DateTime.UtcNow;
                }

                var backlogOptions = scope.ServiceProvider.GetRequiredService<IOptions<ReceiptBacklogMonitorOptions>>();
                if ((DateTime.UtcNow - _lastBacklogSnapshotUtc).TotalMinutes >= backlogOptions.Value.SnapshotIntervalMinutes)
                {
                    var backlogMonitor = scope.ServiceProvider.GetRequiredService<IReceiptBacklogMonitor>();
                    var backlogStatus = await backlogMonitor.GetStatusAsync(stoppingToken);
                    _lastBacklogSnapshotUtc = DateTime.UtcNow;

                    if (backlogStatus.HasAlert)
                    {
                        _logger.LogWarning("Receipt backlog alert {@Status}", backlogStatus);
                    }
                    else
                    {
                        _logger.LogInformation("Receipt backlog snapshot {@Snapshot}", backlogStatus.Snapshot);
                    }
                }

                if (normalized == 0 && extracted == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.Value.IdleDelaySeconds), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR worker loop encountered an unexpected error.");
                await Task.Delay(TimeSpan.FromSeconds(_options.Value.IdleDelaySeconds), stoppingToken);
            }
        }
    }
}
