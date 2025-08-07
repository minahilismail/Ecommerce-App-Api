namespace Ecommerce_Api.Models
{
    public class UserRole : AuditableEntity
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; } // Optional: role expiration
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public User User { get; set; }
        public Role Role { get; set; }
    }
}
