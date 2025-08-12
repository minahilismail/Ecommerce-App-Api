using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Api.Models.Dto
{
    public class AddCategory
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public List<CategoryDTO>? SubCategories { get; set; }
    }
}
