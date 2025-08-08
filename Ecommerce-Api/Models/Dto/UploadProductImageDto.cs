using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Api.Models.Dto
{
    public class UploadProductImageDto
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public IFormFile Image { get; set; }
    }
}
