namespace Ecommerce_Api.Models
{
    public class AuditableEntity
    {
        public virtual DateTime CreatedOn { get; set; }
        public virtual DateTime CreatedBy { get; set; }
        public virtual DateTime UpdatedOn { get; set; }
        public virtual DateTime UpdatedBy { get; set; }
    }
}
