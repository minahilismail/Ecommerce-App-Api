using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Api.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        public string Uid { get; set; }

        public int? ParentRoleId { get; set; }
        public Role? ParentRole { get; set; }

    }
}
