using Ecommerce_Api.Data;
using Ecommerce_Api.Models;
using Ecommerce_Api.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var categories = _db.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .ToList();

            var categoryDTOs = categories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory?.Name,
                StatusId = c.StatusId,
                Level = c.Level,
                SubCategories = c.SubCategories.Select(sub => new CategoryDTO
                {
                    Id = sub.Id,
                    Name = sub.Name,
                    Code = sub.Code,
                    Description = sub.Description,
                    ParentCategoryId = sub.ParentCategoryId,
                    StatusId = sub.StatusId,
                    Level = sub.Level
                }).ToList()
            }).ToList();

            return Ok(categoryDTOs);
        }

        [HttpGet("root")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CategoryDTO>> GetRootCategories()
        {
            var rootCategories = _db.Categories
                .Where(c => c.ParentCategoryId == null)
                .Include(c => c.SubCategories)
                .ToList();

            var categoryDTOs = rootCategories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                StatusId = c.StatusId,
                Level = c.Level,
                SubCategories = c.SubCategories.Select(sub => new CategoryDTO
                {
                    Id = sub.Id,
                    Name = sub.Name,
                    Code = sub.Code,
                    Description = sub.Description,
                    ParentCategoryId = sub.ParentCategoryId,
                    StatusId = sub.StatusId,
                    Level = sub.Level,
                }).ToList()
            }).ToList();

            return Ok(categoryDTOs);
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

            var category = _db.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .FirstOrDefault(u => u.Id == id);

            if (category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }

            var categoryDTO = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                StatusId = category.StatusId,
                Level = category.Level,
                SubCategories = category.SubCategories.Select(sub => new CategoryDTO
                {
                    Id = sub.Id,
                    Name = sub.Name,
                    Code = sub.Code,
                    Description = sub.Description,
                    ParentCategoryId = sub.ParentCategoryId,
                    StatusId = sub.StatusId,
                    Level = sub.Level
                }).ToList()
            };

            return Ok(categoryDTO);
        }
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CategoryDTO> CreateCategory([FromBody] CategoryDTO categoryDTO)
        {
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

            // Validate parent category exists if ParentCategoryId is provided
            if (categoryDTO.ParentCategoryId.HasValue)
            {
                var parentExists = _db.Categories.Any(c => c.Id == categoryDTO.ParentCategoryId.Value);
                if (!parentExists)
                {
                    ModelState.AddModelError("ParentCategoryId", "Parent category does not exist.");
                    return BadRequest(ModelState);
                }
            }

            Category model = new()
            {
                Name = categoryDTO.Name,
                Code = categoryDTO.Code,
                Description = categoryDTO.Description,
                ParentCategoryId = categoryDTO.ParentCategoryId,
                StatusId = categoryDTO.StatusId ?? 1,
                Level = categoryDTO.Level,
                ParentCategory = categoryDTO.ParentCategoryId.HasValue
                ? _db.Categories.FirstOrDefault(c => c.Id == categoryDTO.ParentCategoryId.Value)
                : null,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            _db.Categories.Add(model);
            _db.SaveChanges();

            categoryDTO.Id = model.Id;
            return CreatedAtRoute("GetCategory", new { id = model.Id }, categoryDTO);
        }



        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id:int}", Name = "DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteCategory(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var category = _db.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefault(u => u.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            _db.Categories.Remove(category);
            _db.SaveChanges();
            return NoContent();
        }
        [Authorize(Roles = "Administrator")]
        [HttpPut("{id:int}", Name = "UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateCategory(int id, [FromBody] CategoryDTO categoryDTO)
        {
            if (categoryDTO == null || id != categoryDTO.Id)
            {
                return BadRequest();
            }

            // Validate parent category exists if ParentCategoryId is provided
            if (categoryDTO.ParentCategoryId.HasValue)
            {
                var parentExists = _db.Categories.Any(c => c.Id == categoryDTO.ParentCategoryId.Value);
                if (!parentExists)
                {
                    ModelState.AddModelError("ParentCategoryId", "Parent category does not exist.");
                    return BadRequest(ModelState);
                }

                // Prevent self-reference
                if (categoryDTO.ParentCategoryId == categoryDTO.Id)
                {
                    ModelState.AddModelError("ParentCategoryId", "Category cannot be its own parent.");
                    return BadRequest(ModelState);
                }
            }
            //verify if any existing category contains the same name or code
            if (_db.Categories.Any(c => c.Id != id && (c.Name.ToLower() == categoryDTO.Name.ToLower() || c.Code.ToLower() == categoryDTO.Code.ToLower())))
            {
                ModelState.AddModelError("CustomError", "Category with the same name or code already exists!");
                return BadRequest(ModelState);
            }

            Category model = new()
            {
                Id = categoryDTO.Id,
                Name = categoryDTO.Name,
                Code = categoryDTO.Code,
                Description = categoryDTO.Description,
                ParentCategoryId = categoryDTO.ParentCategoryId,
                StatusId = categoryDTO.StatusId,
                Level = categoryDTO.Level,
                UpdatedDate = DateTime.Now
            };

            _db.Categories.Update(model);
            _db.SaveChanges();
            return NoContent();
        }

        [HttpGet("status/{statusId:int}", Name = "GetCategoryByStatusId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CategoryDTO> GetCategoryByStatusId(int statusId)
        {
            if (statusId == 0)
            {
                return BadRequest("Invalid Status Id.");
            }

            var categories = _db.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Where(c => c.StatusId == statusId)
                .ToList();



            var categoryDTOs = categories.Select(category => new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                Level = category.Level,
                StatusId = category.StatusId,
                SubCategories = category.SubCategories.Select(sub => new CategoryDTO
                {
                    Id = sub.Id,
                    Name = sub.Name,
                    Code = sub.Code,
                    Description = sub.Description,
                    ParentCategoryId = sub.ParentCategoryId,
                    StatusId = sub.StatusId,
                    Level = sub.Level
                }).ToList()
            }).ToList();

            return Ok(categoryDTOs);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPatch("{id:int}/status/{statusId:int}", Name = "UpdateCategoryStatus")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateCategoryStatus(int id, int statusId)
        {

            // Validate statusId exists first
            var statusExists = _db.Status.Any(s => s.Id == statusId);
            if (!statusExists)
            {
                ModelState.AddModelError("StatusId", "Status does not exist.");
                return BadRequest(ModelState);
            }

            var category = _db.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            category.StatusId = statusId;
            category.UpdatedDate = DateTime.Now;

            _db.SaveChanges();
            return NoContent();
        }
    }
}