using Ecommerce_Api.Data;
using Ecommerce_Api.Models;
using Ecommerce_Api.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CategoryDTO>> GetCategories()
        {
            return Ok(_db.Categories.ToList());
        }

        [HttpGet("{id:int}", Name = "GetCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CategoryDTO> GetCategory(int id)
        {
            if (id == 0)
            {
                return BadRequest("Category ID cannot be zero.");
            }
            var category = _db.Categories.FirstOrDefault(u => u.Id == id);
            if (category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }
            return Ok(category);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CategoryDTO> CreateCategory([FromBody] CategoryDTO categoryDTO)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            if (_db.Categories.FirstOrDefault(u => u.Name.ToLower() == categoryDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Category already exists!");
                return BadRequest(ModelState);
            }
            if (categoryDTO == null)
            {
                return BadRequest(categoryDTO);
            }

            if (categoryDTO.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            Category model = new()
            {
                Name = categoryDTO.Name,
                Code = categoryDTO.Code,
                Description = categoryDTO.Description,
            };
            _db.Categories.Add(model);
            _db.SaveChanges();
            return CreatedAtRoute("GetCategory", new { id = categoryDTO.Id }, categoryDTO);
        }

        [HttpDelete("{id:int}", Name = "DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVilla(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var category = _db.Categories.FirstOrDefault(u => u.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            _db.Categories.Remove(category);
            _db.SaveChanges();
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateCategory(int id, [FromBody] CategoryDTO categoryDTO)
        {
            if (categoryDTO == null || id != categoryDTO.Id)
            {
                return BadRequest();
            }

            Category model = new()
            {
                Id = categoryDTO.Id,
                Name = categoryDTO.Name,
                Code = categoryDTO.Code,
                Description = categoryDTO.Description,
            };
            _db.Categories.Update(model);
            _db.SaveChanges();
            return NoContent();
        }
    }
}
