using Ecommerce_Api.Data;
using Ecommerce_Api.Models;
using Ecommerce_Api.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ProductDTO>> GetProducts()
        {
            var products = _db.Products
                .Include(p => p.Category)
                .ToList();

            var productDTOs = products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                Description = p.Description,
                Image = p.Image,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name
            }).ToList();

            return Ok(productDTOs);
        }

        [HttpGet("{id:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProductDTO> GetProduct(int id)
        {
            if (id == 0)
            {
                return BadRequest("Product ID cannot be zero.");
            }

            var product = _db.Products
                .Include(p => p.Category)
                .FirstOrDefault(u => u.Id == id);

            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            var productDTO = new ProductDTO
            {
                Id = product.Id,
                Title = product.Title,
                Price = product.Price,
                Description = product.Description,
                Image = product.Image,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name
            };

            return Ok(productDTO);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProductDTO> CreateProduct([FromBody] ProductDTO productDTO)
        {
            
            if(_db.Categories.FirstOrDefault(c => c.Id == productDTO.CategoryId) == null)
            {
                ModelState.AddModelError("CustomError", "Category does not exist!");
                return BadRequest(ModelState);
            }

            if (productDTO == null)
            {
                return BadRequest(productDTO);
            }

            if (productDTO.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

           
            Product model = new()
            {
                Title = productDTO.Title,
                Price = productDTO.Price,
                Description = productDTO.Description,
                Image = productDTO.Image,
                CategoryId = productDTO.CategoryId,
            };

            _db.Products.Add(model);
            _db.SaveChanges();

            productDTO.Id = model.Id;
            return CreatedAtRoute("GetProduct", new { id = model.Id }, productDTO);
        }

        [HttpDelete("{id:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteProduct(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var product = _db.Products
                .Include(c => c.Category)
                .FirstOrDefault(u => u.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            _db.Products.Remove(product);
            _db.SaveChanges();
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateProduct(int id, [FromBody] ProductDTO productDTO)
        {
            if (productDTO == null || id != productDTO.Id)
            {
                return BadRequest();
            }

            // Validate category exists
            if (_db.Products.FirstOrDefault(c => c.Id == productDTO.Id) == null)
            {
                ModelState.AddModelError("ProductId", "Product does not exist.");
                return BadRequest(ModelState);
            }

            var product = _db.Products.FirstOrDefault(c => c.Id == productDTO.Id);
            if (product == null)
            {
                ModelState.AddModelError("ProductId", "Product not found.");
                return NotFound(ModelState);
            }

            product.Title = productDTO.Title;
            product.Price = productDTO.Price;
            product.Description = productDTO.Description;
            product.Image = productDTO.Image;
            product.CategoryId = productDTO.CategoryId;

            _db.SaveChanges();
            return NoContent();
         }

        [HttpGet("category/{categoryId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ProductDTO>> GetProductByCategory(int categoryId)
        {
            if (categoryId == 0)
            {
                return BadRequest("Category ID cannot be zero.");
            }
            var products = _db.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToList();
            if (!products.Any())
            {
                return NotFound($"No products found for category ID {categoryId}.");
            }
            var productDTOs = products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                Description = p.Description,
                Image = p.Image,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name
            }).ToList();
            return Ok(productDTOs);
        }

    }
}
