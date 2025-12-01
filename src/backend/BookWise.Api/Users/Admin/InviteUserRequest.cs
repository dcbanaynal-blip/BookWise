using System.ComponentModel.DataAnnotations;

namespace BookWise.Api.Users.Admin;

public class InviteUserRequest
{
    [Required]
    [MaxLength(255)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(40)]
    public string Role { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "At least one email must be provided.")]
    public List<string> Emails { get; set; } = new();
}
