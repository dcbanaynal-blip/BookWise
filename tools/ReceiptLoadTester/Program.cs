namespace ReceiptLoadTester;

using BookWise.Infrastructure.Receipts;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var options = ReceiptLoadOptions.FromArgs(args);

        Console.WriteLine("BookWise Receipt Load Tester");
        Console.WriteLine($"- Receipts: {options.TotalReceipts}");
        Console.WriteLine($"- Parallel uploads: {options.ParallelUploads}");
        Console.WriteLine($"- Receipt size: {options.ReceiptSizeBytes / 1024 / 1024.0:F1} MB");
        Console.WriteLine($"- OCR batch size: {options.BatchSize}");
        Console.WriteLine();

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            Console.WriteLine("Cancellation requested...");
            cts.Cancel();
            eventArgs.Cancel = true;
        };

        await using var harness = new ReceiptLoadHarness(options);

        var result = await harness.RunAsync(cts.Token);

        Console.WriteLine();
        Console.WriteLine("===== Load Test Summary =====");
        Console.WriteLine($"Receipt uploads time: {result.UploadDuration:c} ({result.UploadThroughput:F2} receipts/sec)");
        Console.WriteLine($"OCR processing time : {result.OcrDuration:c} ({result.OcrThroughput:F2} receipts/sec)");
        Console.WriteLine($"Total elapsed time  : {result.TotalDuration:c}");
        Console.WriteLine($"Max queue depth     : {result.MaxQueueDepth}");
        Console.WriteLine($"Total failures      : {result.Failures}");
        Console.WriteLine($"Average CPU delay   : preprocess {options.PreprocessDelayMs} ms, extract {options.ExtractDelayMs} ms");
        Console.WriteLine("=============================");
    }
}
