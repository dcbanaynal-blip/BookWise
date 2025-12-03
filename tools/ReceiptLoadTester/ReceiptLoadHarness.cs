namespace ReceiptLoadTester;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using BookWise.Application.Receipts;
using BookWise.Domain.Entities;
using BookWise.Infrastructure.Persistence;
using BookWise.Infrastructure.Receipts;
using Microsoft.EntityFrameworkCore;

public sealed class ReceiptLoadHarness : IAsyncDisposable
{
    private readonly ReceiptLoadOptions _options;
    private readonly DbContextOptions<BookWiseDbContext> _dbOptions;
    private readonly InMemoryReceiptFileStorage _fileStorage = new();
    private readonly BookWiseDbContext _pipelineContext;
    private readonly SimulatedReceiptOcrPipeline _pipeline;
    private readonly byte[] _samplePayload;

    public ReceiptLoadHarness(ReceiptLoadOptions options)
    {
        _options = options;
        _dbOptions = new DbContextOptionsBuilder<BookWiseDbContext>()
            .UseInMemoryDatabase($"ReceiptLoadTests-{Guid.NewGuid()}")
            .Options;
        _pipelineContext = new BookWiseDbContext(_dbOptions);
        _pipeline = new SimulatedReceiptOcrPipeline(_pipelineContext, options);
        _samplePayload = GeneratePayload(options.ReceiptSizeBytes);
    }

    public async Task<ReceiptLoadResult> RunAsync(CancellationToken cancellationToken)
    {
        var uploadSw = Stopwatch.StartNew();
        var failures = 0;

        var uploadRange = Enumerable.Range(0, _options.TotalReceipts);
        await Parallel.ForEachAsync(
            uploadRange,
            new ParallelOptions { MaxDegreeOfParallelism = _options.ParallelUploads, CancellationToken = cancellationToken },
            async (index, token) =>
            {
                try
                {
                    await using var context = new BookWiseDbContext(_dbOptions);
                    var queue = new ReceiptProcessingQueue(context);
                    var service = new ReceiptsService(context, _fileStorage, queue);

                    var payload = CreatePayloadCopy();
                    var model = new CreateReceiptModel
                    {
                        UploadedBy = Guid.NewGuid(),
                        OriginalFileName = $"load-test-{index}.jpg",
                        ContentType = "image/jpeg",
                        FileSize = payload.Length,
                        FileBytes = payload,
                        DocumentDate = DateTime.UtcNow.Date.AddDays(-(index % 30)),
                        SellerName = $"Vendor #{index % 10}",
                        SellerTaxId = $"TX-{index:D6}",
                        SellerAddress = "123 Test St",
                        CustomerName = "Load Test Customer",
                        Notes = "Load test run",
                        IsVatApplicable = index % 2 == 0
                    };

                    await service.CreateReceiptAsync(model, token);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failures);
                    Console.WriteLine($"Upload failed for {index}: {ex.Message}");
                }
            });
        uploadSw.Stop();

        var metricsContext = new BookWiseDbContext(_dbOptions);
        var maxQueueDepth = await metricsContext.ReceiptProcessingJobs.CountAsync(cancellationToken);
        await metricsContext.DisposeAsync();

        var uploadThroughput = _options.TotalReceipts / Math.Max(uploadSw.Elapsed.TotalSeconds, 0.001);

        var ocrSw = Stopwatch.StartNew();
        var processedReceipts = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var processed = await _pipeline.PreprocessPendingReceiptsAsync(_options.BatchSize, cancellationToken);
            if (processed == 0)
            {
                break;
            }
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var processed = await _pipeline.ExtractReceiptContentAsync(_options.BatchSize, cancellationToken);
            if (processed == 0)
            {
                break;
            }

            processedReceipts += processed;
        }

        ocrSw.Stop();

        var ocrThroughput = processedReceipts / Math.Max(ocrSw.Elapsed.TotalSeconds, 0.001);

        return new ReceiptLoadResult(
            _options.TotalReceipts,
            uploadSw.Elapsed,
            ocrSw.Elapsed,
            TimeSpan.FromTicks(uploadSw.ElapsedTicks + ocrSw.ElapsedTicks),
            uploadThroughput,
            ocrThroughput,
            maxQueueDepth,
            failures);
    }

    public async ValueTask DisposeAsync()
    {
        await _pipelineContext.DisposeAsync();
    }

    private static byte[] GeneratePayload(int sizeBytes)
    {
        var buffer = GC.AllocateUninitializedArray<byte>(sizeBytes);
        RandomNumberGenerator.Fill(buffer);
        return buffer;
    }

    private byte[] CreatePayloadCopy()
    {
        var clone = GC.AllocateUninitializedArray<byte>(_samplePayload.Length);
        Buffer.BlockCopy(_samplePayload, 0, clone, 0, _samplePayload.Length);
        return clone;
    }

    private sealed class InMemoryReceiptFileStorage : IReceiptFileStorage
    {
        public Task<ReceiptFileSaveResult> SaveAsync(ReceiptFilePayload payload, CancellationToken cancellationToken)
        {
            var clone = GC.AllocateUninitializedArray<byte>(payload.Data.Length);
            Buffer.BlockCopy(payload.Data, 0, clone, 0, payload.Data.Length);
            return Task.FromResult(new ReceiptFileSaveResult(clone, payload.ContentType));
        }
    }
}

public sealed record ReceiptLoadResult(
    int TotalReceipts,
    TimeSpan UploadDuration,
    TimeSpan OcrDuration,
    TimeSpan TotalDuration,
    double UploadThroughput,
    double OcrThroughput,
    int MaxQueueDepth,
    int Failures);

public sealed record ReceiptLoadOptions
{
    public int TotalReceipts { get; init; } = 200;
    public int ParallelUploads { get; init; } = Math.Max(Environment.ProcessorCount * 2, 8);
    public int ReceiptSizeBytes { get; init; } = 2 * 1024 * 1024;
    public int BatchSize { get; init; } = 25;
    public int PreprocessDelayMs { get; init; } = 10;
    public int ExtractDelayMs { get; init; } = 15;

    public static ReceiptLoadOptions FromArgs(string[] args)
    {
        var map = ParseArgs(args);
        return new ReceiptLoadOptions
        {
            TotalReceipts = GetInt(map, "receipts", 200),
            ParallelUploads = GetInt(map, "parallel", Math.Max(Environment.ProcessorCount * 2, 8)),
            ReceiptSizeBytes = map.TryGetValue("size-mb", out var sizeMb)
                ? (int)(double.Parse(sizeMb) * 1024 * 1024)
                : GetInt(map, "size-bytes", 2 * 1024 * 1024),
            BatchSize = GetInt(map, "batch", 25),
            PreprocessDelayMs = GetInt(map, "preprocess-delay-ms", 10),
            ExtractDelayMs = GetInt(map, "extract-delay-ms", 15)
        };
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            var token = args[i];
            if (!token.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var key = token[2..];
            var value = string.Empty;
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                value = args[i + 1];
                i++;
            }

            result[key] = value;
        }

        return result;
    }

    private static int GetInt(Dictionary<string, string> map, string key, int fallback)
    {
        if (map.TryGetValue(key, out var value) && int.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return fallback;
    }
}
