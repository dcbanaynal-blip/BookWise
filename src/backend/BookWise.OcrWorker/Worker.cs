using BookWise.Infrastructure.Ocr;
using Microsoft.Extensions.Options;

namespace BookWise.OcrWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<OcrWorkerOptions> _options;

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

            try
            {
                var normalized = await pipeline.PreprocessPendingReceiptsAsync(
                    _options.Value.BatchSize,
                    stoppingToken);

                var extracted = await pipeline.ExtractReceiptContentAsync(
                    _options.Value.BatchSize,
                    stoppingToken);

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
