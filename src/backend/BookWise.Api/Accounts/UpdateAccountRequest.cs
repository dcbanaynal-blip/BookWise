using System.ComponentModel.DataAnnotations;
using BookWise.Domain.Entities;

namespace BookWise.Api.Accounts;

public sealed class UpdateAccountRequest
{
    [Required]
    [MaxLength(120)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string SegmentCode { get; init; } = string.Empty;

    [Required]
    public AccountType Type { get; init; }
}
