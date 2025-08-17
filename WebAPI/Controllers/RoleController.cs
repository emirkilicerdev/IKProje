using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Data;
using WebAPI.Models;
using System.Linq;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // All actions in this controller require 'Admin' role
    public class RoleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Role/all
        // Retrieves all roles.
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<RoleModel>>> GetAllRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return Ok(roles);
        }

        // GET: api/Role/{id}
        // Retrieves a specific role by ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleModel>> GetRoleById(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound(new { Message = "Role not found." });
            }
            return Ok(role);
        }

        // POST: api/Role
        // Creates a new role.
        [HttpPost]
        public async Task<ActionResult<RoleModel>> CreateRole([FromBody] RoleModel role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
    
            if (await _context.Roles.AnyAsync(r => r.Name == role.Name))
            {
                return Conflict(new { Message = $"A role named '{role.Name}' already exists." });
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }

        // PUT: api/Role/{id}
        // Updates an existing role.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleModel role)
        {
            if (id != role.Id)
            {
                return BadRequest(new { Message = "Route ID does not match body ID." });
            }

            // Check if the role exists in the database
            var existingRole = await _context.Roles.FindAsync(id);
            if (existingRole == null)
            {
                return NotFound(new { Message = "Role to update not found." });
            }

            // Check for uniqueness of the role name (excluding itself)
            if (await _context.Roles.AnyAsync(r => r.Name == role.Name && r.Id != id))
            {
                return Conflict(new { Message = $"Another role named '{role.Name}' already exists." });
            }

            existingRole.Name = role.Name; // Only update the name

            try
            {
                _context.Roles.Update(existingRole);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Roles.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // DELETE: api/Role/{id}
        // Deletes a role.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound(new { Message = "Role to delete not found." });
            }

            // Check if this role is assigned to any user
            var hasUsers = await _context.UserRoles.AnyAsync(ur => ur.RoleId == id);
            if (hasUsers)
            {
                return BadRequest(new { Message = "This role is assigned to active users and cannot be deleted. Please change the roles of users using this role first." });
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
