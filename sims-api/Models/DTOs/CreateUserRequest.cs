using System.ComponentModel.DataAnnotations;

namespace sims.Models.DTOs;

public class CreateUserRequest
{
    [Required]
    public string Firstname { get; set; }
    [Required]
    public string Lastname { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }
    [Required]
    public int? RoleId { get; set; }
    [Required]
    public string Password { get; set; }
}