using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using WebAPI.Data;
using WebAPI.Models;
using WebAPI.Dtos;
using System.Collections.Generic;
using System.Linq;
using BCrypt.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Select(u => new UserDetailDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    RoleIds = u.UserRoles.Select(ur => ur.RoleId).ToList(),
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDetailDto>> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound(new { Message = "User not found." });

            var currentUserId = GetCurrentUserIdFromClaims();
            var isAdmin = User.IsInRole("Admin");

            if (currentUserId == null) return Unauthorized(new { Message = "Current user ID not found or invalid." });
            if (!isAdmin && currentUserId != id) return Forbid("You are not authorized to view this user profile.");

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                RoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList(),
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDetailDto>> CreateUser([FromBody] UserCreateDto createUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
                return BadRequest(new { Message = "This username is already taken." });

            if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
                return BadRequest(new { Message = "This email address is already in use." });

            if (createUserDto.RoleIds == null || !createUserDto.RoleIds.Any())
                return BadRequest(new { Message = "At least one role ID must be specified." });

            var existingRoleIds = await _context.Roles
                .Where(r => createUserDto.RoleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync();

            var invalidRoleIds = createUserDto.RoleIds.Except(existingRoleIds).ToList();
            if (invalidRoleIds.Any())
                return BadRequest(new { Message = $"Invalid role IDs: {string.Join(", ", invalidRoleIds)}" });

            var newUser = new UserModel
            {
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                CreatedAt = DateTime.UtcNow
            };

            foreach (var roleId in createUserDto.RoleIds)
                newUser.UserRoles.Add(new UserRoleModel { RoleId = roleId });

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            await _context.Entry(newUser).Collection(u => u.UserRoles).LoadAsync();
            foreach (var ur in newUser.UserRoles)
                await _context.Entry(ur).Reference(x => x.Role).LoadAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, new UserDetailDto
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                Roles = newUser.UserRoles.Select(ur => ur.Role.Name).ToList(),
                RoleIds = newUser.UserRoles.Select(ur => ur.RoleId).ToList(),
                CreatedAt = newUser.CreatedAt
            });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto updateUserDto)
        {
            var currentUserId = GetCurrentUserIdFromClaims();
            var isAdmin = User.IsInRole("Admin");
            if (currentUserId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            if (!isAdmin && currentUserId != id) return Forbid();

            if (!string.IsNullOrEmpty(updateUserDto.Username) && user.Username != updateUserDto.Username)
            {
                if (await _context.Users.AnyAsync(u => u.Username == updateUserDto.Username && u.Id != id))
                    return BadRequest(new { Message = "Username already taken." });
                user.Username = updateUserDto.Username;
            }

            if (!string.IsNullOrEmpty(updateUserDto.Email) && user.Email != updateUserDto.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == updateUserDto.Email && u.Id != id))
                    return BadRequest(new { Message = "Email already in use." });
                user.Email = updateUserDto.Email;
            }

            if (!string.IsNullOrEmpty(updateUserDto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);

            if (isAdmin && updateUserDto.RoleIds != null)
            {
                _context.UserRoles.RemoveRange(user.UserRoles);
                user.UserRoles.Clear();

                var newRoles = await _context.Roles.Where(r => updateUserDto.RoleIds.Contains(r.Id)).ToListAsync();
                foreach (var role in newRoles)
                    user.UserRoles.Add(new UserRoleModel { UserId = user.Id, Role = role });
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await _context.Entry(user).Collection(u => u.UserRoles).LoadAsync();
            foreach (var ur in user.UserRoles)
                await _context.Entry(ur).Reference(x => x.Role).LoadAsync();

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                RoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList(),
                CreatedAt = user.CreatedAt
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.LeaveRequests) // Kullanıcının izinlerini yükle
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound(new { Message = "User not found." });

            // Cascade delete: UserRoles ve LeaveRequests otomatik silinecek
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userId = GetCurrentUserIdFromClaims();
            if (userId == null) return Unauthorized("User ID not found.");

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("User not found.");

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                RoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList(),
                CreatedAt = user.CreatedAt
            });
        }
    }
}
