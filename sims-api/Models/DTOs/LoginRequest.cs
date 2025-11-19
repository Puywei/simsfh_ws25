using System.ComponentModel.DataAnnotations;

namespace sims.Models.DTOs
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}