using System.ComponentModel.DataAnnotations;

namespace BookWise.Api.Users.Admin;

public class UpdateUserStatusRequest
{
    [Required]
    public bool IsActive { get; set; }
}
