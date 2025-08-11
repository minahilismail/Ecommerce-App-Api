using Ecommerce_Api.Data;
using Ecommerce_Api.Models;
using Ecommerce_Api.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<UserDto>> GetUsers()
        {
            var users = _db.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .ToList();

            var userDTOs = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Username = u.Username,
                Email = u.Email,
                IsActive = u.IsActive,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToArray()
            }).ToList();

            return Ok(userDTOs);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("{id:int}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {

            var user = _db.Users.Include(u => u.UserRoles).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Check for duplicate email or username
            if (_db.Users.Any(u => u.Id != id && (u.Email.ToLower() == updateUserDto.Email.ToLower() || u.Username.ToLower() == updateUserDto.Username.ToLower())))
            {
                ModelState.AddModelError("CustomError", "User with the same email or username already exists!");
                return BadRequest(ModelState);
            }

            // Validate that all provided role IDs exist
            if (updateUserDto.RoleIds != null && updateUserDto.RoleIds.Any())
            {
                var existingRoleIds = _db.Roles.Select(r => r.Id).ToList();
                var invalidRoleIds = updateUserDto.RoleIds.Except(existingRoleIds).ToList();
                if (invalidRoleIds.Any())
                {
                    ModelState.AddModelError("RoleIds", $"Invalid role IDs: {string.Join(", ", invalidRoleIds)}");
                    return BadRequest(ModelState);
                }
            }

            user.Name = updateUserDto.Name;
            user.Username = updateUserDto.Username;
            user.Email = updateUserDto.Email;
            user.IsActive = updateUserDto.IsActive;

            // Update roles if provided
            if (updateUserDto.RoleIds != null)
            {
                // Remove existing roles
                var existingRoles = user.UserRoles.ToList();
                foreach (var existingRole in existingRoles)
                {
                    _db.UserRole.Remove(existingRole);
                }

                // Add new roles
                foreach (var roleId in updateUserDto.RoleIds)
                {
                    _db.UserRole.Add(new UserRole { UserId = user.Id, RoleId = roleId });
                }
            }

            _db.SaveChanges();
            return NoContent();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPatch("{id:int}/roles", Name = "UpdateUserRoles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateUserRoles(int id, int[] roleIds)
        {
            var user = _db.Users.Include(u => u.UserRoles).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Remove roles that are no longer assigned
            var rolesToRemove = user.UserRoles.Where(ur => !roleIds.Contains(ur.RoleId)).ToList();
            foreach (var role in rolesToRemove)
            {
                _db.UserRole.Remove(role);
            }

            // Add new roles
            foreach (var roleId in roleIds)
            {
                if (!user.UserRoles.Any(ur => ur.RoleId == roleId))
                {
                    user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
                }
            }

            _db.SaveChanges();
            return NoContent();
        }

        [HttpGet("roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<RoleDto>> GetRoles()
        {
            var roles = _db.Roles.ToList();

            var roleDTOs = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();

            return Ok(roleDTOs);
        }
    }
}
