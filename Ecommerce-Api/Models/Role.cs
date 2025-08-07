using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Api.Models
{
    public class Role : AuditableEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    }
}
