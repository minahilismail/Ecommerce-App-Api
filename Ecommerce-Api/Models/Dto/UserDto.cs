using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ecommerce_Api.Models.Dto
{
    public class UserDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        //public int RoleId { get; set; }
        //public Role Role { get; set; }

        //public bool IsActive { get; set; } = true;
        //public DateTime CreatedDate { get; set; } = DateTime.Now;
        //public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
