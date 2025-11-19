using System.ComponentModel.DataAnnotations;

namespace sims.Models;

public class ModifyUserRequest
{
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email { get; set; }
    public int? RoleId { get; set; }
    public string? Password { get; set; }
}