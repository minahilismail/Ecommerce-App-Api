using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Api.Models.Dto
{
    public class RegisterRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public int RoleId { get; set; } = 1; // Default to User role
    }
}
