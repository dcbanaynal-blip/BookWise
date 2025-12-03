using System;
using BookWise.Api.Receipts;
using FluentValidation;

namespace BookWise.Api.Receipts;

public class ReceiptUploadRequestValidator : AbstractValidator<ReceiptUploadRequest>
{
    public ReceiptUploadRequestValidator()
    {
        RuleFor(x => x.FileName)
            .MaximumLength(260);

        RuleFor(x => x.ContentType)
            .MaximumLength(120);

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(25_000_000);

        RuleFor(x => x.SellerName)
            .MaximumLength(255);

        RuleFor(x => x.SellerTaxId)
            .MaximumLength(50);

        RuleFor(x => x.SellerAddress)
            .MaximumLength(512);

        RuleFor(x => x.CustomerName)
            .MaximumLength(255);

        RuleFor(x => x.CustomerTaxId)
            .MaximumLength(50);

        RuleFor(x => x.CustomerAddress)
            .MaximumLength(512);

        RuleFor(x => x.Notes)
            .MaximumLength(1024);

        RuleFor(x => x.DocumentDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.DocumentDate.HasValue);
    }

    public static bool IsSupportedMime(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return contentType switch
        {
            "image/jpeg" or "image/png" or "application/pdf" => true,
            _ => false
        };
    }
}
