using System.ComponentModel.DataAnnotations;

namespace JwtPratik.DTOs
{
    public class RegisterRequest
    {
        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
