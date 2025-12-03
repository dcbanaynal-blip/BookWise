using System;
using System.Buffers;
using ImageMagick;
using Microsoft.Extensions.Logging;

namespace BookWise.Infrastructure.Ocr;

public interface IReceiptImagePreprocessor
{
    Task<byte[]> NormalizeAsync(byte[] imageBytes, CancellationToken cancellationToken = default);
}

public class MagickReceiptImagePreprocessor : IReceiptImagePreprocessor
{
    private readonly ILogger<MagickReceiptImagePreprocessor> _logger;

    public MagickReceiptImagePreprocessor(ILogger<MagickReceiptImagePreprocessor> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> NormalizeAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        if (imageBytes.Length == 0)
        {
            return Task.FromResult(imageBytes);
        }

        using var image = new MagickImage(imageBytes);

        image.AutoOrient();
        image.Deskew(new Percentage(1.5));
        image.ColorType = ColorType.Grayscale;
        image.ContrastStretch(new Percentage(0.1), new Percentage(0.9));
        image.Sharpen();
        image.Format = MagickFormat.Png;

        using var collection = new MagickImageCollection();
        collection.Add(image);
        if (!collection[0].HasAlpha)
        {
            collection[0].Alpha(AlphaOption.Opaque);
        }

        var optimized = collection.ToByteArray();
        _logger.LogDebug("Preprocessed image to {Length} bytes", optimized.Length);

        return Task.FromResult(optimized);
    }
}
