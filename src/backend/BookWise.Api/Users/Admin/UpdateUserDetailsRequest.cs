using System.ComponentModel.DataAnnotations;

namespace BookWise.Api.Users.Admin;

public class UpdateUserDetailsRequest
{
    [Required]
    [MaxLength(255)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string LastName { get; set; } = string.Empty;
}
