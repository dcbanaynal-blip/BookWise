using System.ComponentModel.DataAnnotations;

namespace BookWise.Api.Users.Admin;

public class AddUserEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
