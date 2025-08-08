using Ecommerce_Api.Data;
using Ecommerce_Api.Extensions;
using Ecommerce_Api.Models;
using Ecommerce_Api.Models.Dto;
using Ecommerce_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        public ProductController(ApplicationDbContext db, IStorageService storageService, IConfiguration configuration)
        {
            _db = db;
            _storageService = storageService;
            _configuration = configuration;
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

        [HttpGet("paged-products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Models.Dto.PagedResult<ProductDTO>>> GetProductsPaged([FromQuery] PaginationParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var query = _db.Products
                .Include(p => p.Category)
                .AsQueryable();

            var pagedProducts = await query
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Price = p.Price,
                    Description = p.Description,
                    Image = p.Image,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null
                })
                .ToPagedResultAsync(parameters.PageNumber, parameters.PageSize);

            return Ok(pagedProducts);
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
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDTO>> CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            if (_db.Categories.FirstOrDefault(c => c.Id == createProductDto.CategoryId) == null)
            {
                ModelState.AddModelError("CustomError", "Category does not exist!");
                return BadRequest(ModelState);
            }

            if (createProductDto == null)
            {
                return BadRequest("Product data is required.");
            }

            string imageUrl = null;

            // Upload image if provided
            if (createProductDto.Image != null)
            {
                // Validate image file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(createProductDto.Image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Image", "Only image files are allowed (jpg, jpeg, png");
                    return BadRequest(ModelState);
                }

                // Check file size (max 5MB)
                if (createProductDto.Image.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("Image", "File size cannot exceed 5MB.");
                    return BadRequest(ModelState);
                }

                try
                {
                    using (var stream = new MemoryStream())
                    {
                        await createProductDto.Image.CopyToAsync(stream);

                        // Generate unique filename
                        var fileName = $"product_{Guid.NewGuid()}{fileExtension}";
                        var containerName = this._configuration["AzureStorage:ContainerName"];

                        // Upload to Azure Blob Storage
                        imageUrl = await _storageService.UploadAsync(stream.ToArray(), fileName,containerName);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Image", "Failed to upload image. Please try again.");
                    return BadRequest(ModelState);
                }
            }

            Product model = new()
            {
                Title = createProductDto.Title,
                Price = createProductDto.Price,
                Description = createProductDto.Description,
                Image = imageUrl,
                CategoryId = createProductDto.CategoryId,
            };

            _db.Products.Add(model);
            await _db.SaveChangesAsync();

            var productDTO = new ProductDTO
            {
                Id = model.Id,
                Title = model.Title,
                Price = model.Price,
                Description = model.Description,
                Image = model.Image,
                CategoryId = model.CategoryId,
                CategoryName = _db.Categories.FirstOrDefault(c => c.Id == model.CategoryId)?.Name
            };

            return CreatedAtRoute("GetProduct", new { id = model.Id }, productDTO);
        }
        [Authorize(Roles = "Administrator")]
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
        [Authorize(Roles = "Administrator")]
        [HttpPut("{id:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null || id != updateProductDto.Id)
            {
                return BadRequest();
            }

            var product = _db.Products.FirstOrDefault(c => c.Id == id);
            if (product == null)
            {
                ModelState.AddModelError("ProductId", "Product not found.");
                return NotFound(ModelState);
            }

            // Validate category exists
            if (_db.Categories.FirstOrDefault(c => c.Id == updateProductDto.CategoryId) == null)
            {
                ModelState.AddModelError("CategoryId", "Category does not exist.");
                return BadRequest(ModelState);
            }

            string imageUrl = product.Image; // Keep existing image by default

            // Upload new image if provided
            if (updateProductDto.Image != null)
            {
                // Validate image file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(updateProductDto.Image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Image", "Only image files are allowed (jpg, jpeg, png).");
                    return BadRequest(ModelState);
                }

                if (updateProductDto.Image.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("Image", "File size cannot exceed 5MB.");
                    return BadRequest(ModelState);
                }

                try
                {
                    using (var stream = new MemoryStream())
                    {
                        await updateProductDto.Image.CopyToAsync(stream);

                        var fileName = $"product_{id}_{Guid.NewGuid()}{fileExtension}";
                        imageUrl = await _storageService.UploadAsync(stream.ToArray(), fileName);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Image", "Failed to upload image. Please try again.");
                    return BadRequest(ModelState);
                }
            }

            product.Title = updateProductDto.Title;
            product.Price = updateProductDto.Price;
            product.Description = updateProductDto.Description;
            product.Image = imageUrl; // Set the image URL, not the IFormFile
            product.CategoryId = updateProductDto.CategoryId;

            await _db.SaveChangesAsync();
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

        [Authorize(Roles = "Administrator")]
        [HttpPost("uploadImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadProductImage([FromForm] UploadProductImageDto model)
        {
            var product = await _db.Products.FindAsync(model.ProductId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            if (model.Image == null)
            {
                return BadRequest("Image file is required.");
            }

            // Validate image file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(model.Image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Only image files are allowed (jpg, jpeg, png).");
            }

            if (model.Image.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size cannot exceed 5MB.");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await model.Image.CopyToAsync(stream);

                    var fileName = $"product_{model.ProductId}_{Guid.NewGuid()}{fileExtension}";

                    var pictureUrl = await _storageService.UploadAsync(stream.ToArray(), fileName);

                    // Update the product image URL in the database
                    product.Image = pictureUrl;
                    await _db.SaveChangesAsync();

                    return Ok(new { imageUrl = pictureUrl });
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to upload image.");
            }
        }
    }
}
