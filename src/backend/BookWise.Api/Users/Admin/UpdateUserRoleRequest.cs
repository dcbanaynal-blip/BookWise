using System.ComponentModel.DataAnnotations;

namespace BookWise.Api.Users.Admin;

public class UpdateUserRoleRequest
{
    [Required]
    [MaxLength(40)]
    public string Role { get; set; } = string.Empty;
}
