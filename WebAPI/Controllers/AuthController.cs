using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI.Data;
using WebAPI.Dtos; 
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
            {
                return Conflict(new AuthResponseModel
                {
                    Message = "Kullanıcı adı veya e-posta zaten kullanımda.",
                    Success = false
                });
            }

            var defaultUserRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (defaultUserRole == null)
            {
                return StatusCode(500, new AuthResponseModel
                {
                    Message = "Varsayılan 'User' rolü veritabanında bulunamadı. Lütfen veritabanı seed'ini kontrol edin.",
                    Success = false
                });
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var newUser = new UserModel
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
            };

            newUser.UserRoles.Add(new UserRoleModel { RoleId = defaultUserRole.Id });

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = newUser.Id },
                                   new AuthResponseModel
                                   {
                                       Message = "Kullanıcı başarıyla kaydedildi.",
                                       Success = true,
                                       Data = new AuthResponseData { UserId = newUser.Id }
                                   });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                                     .Include(u => u.UserRoles)
                                         .ThenInclude(ur => ur.Role)
                                     .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return Unauthorized(new AuthResponseModel
                {
                    Message = "Kullanıcı adı veya şifre yanlış.",
                    Success = false
                });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var expires = DateTime.UtcNow.AddHours(1); // Token geçerlilik süresi

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };
            // Kullanıcının sahip olduğu tüm rolleri token'a claim olarak ekle
            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires, // Geçerlilik süresini ata
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new AuthResponseModel
            {
                Message = "Giriş başarılı!",
                Success = true,
                Data = new AuthResponseData
                {
                    Token = tokenString,
                    Expiration = expires, // Frontend için expiration bilgisini gönder
                    Username = user.Username,
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList() // Kullanıcının tüm rollerini gönder
                }
            });
        }

        // Yeni endpoint: Kullanıcının rol seçimi yapması için
        [Authorize] // Bu endpoint'e sadece kimliği doğrulanmış kullanıcılar erişebilir
        [HttpPost("select-role")]
        public async Task<IActionResult> SelectRole([FromBody] SelectRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Mevcut token'dan kullanıcı ID'sini al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new AuthResponseModel { Message = "Kullanıcı kimliği bulunamadı veya geçersiz.", Success = false });
            }

            var user = await _context.Users
                                     .Include(u => u.UserRoles)
                                         .ThenInclude(ur => ur.Role)
                                     .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Unauthorized(new AuthResponseModel { Message = "Kullanıcı bulunamadı.", Success = false });
            }

            var userRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            // Kullanıcının gerçekten bu role sahip olup olmadığını kontrol et
            if (!userRoles.Contains(request.SelectedRole))
            {
                return BadRequest(new AuthResponseModel
                {
                    Message = $"Seçilen rol '{request.SelectedRole}', kullanıcının sahip olduğu roller arasında değil.",
                    Success = false
                });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var expires = DateTime.UtcNow.AddHours(1); // Yeni token için geçerlilik süresi

            // Yeni token için sadece seçilen rolü içeren claim'leri oluştur
            var newClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, request.SelectedRole) // Sadece seçilen rolü ekle
            };


            var newTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(newClaims),
                Expires = expires,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var newToken = tokenHandler.CreateToken(newTokenDescriptor);
            var newTokenString = tokenHandler.WriteToken(newToken);

            return Ok(new AuthResponseModel
            {
                Message = $"Rol başarıyla '{request.SelectedRole}' olarak güncellendi.",
                Success = true,
                Data = new AuthResponseData
                {
                    Token = newTokenString,
                    Expiration = expires,
                    Username = user.Username,
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = userRoles // Kullanıcının tüm rollerini tekrar gönder (değişmedi)
                }
            });
        }

        [HttpGet("currentUserId")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { Message = "Kullanıcı kimliği bulunamadı veya geçersiz." });
            }
            if (int.TryParse(userIdClaim, out int userId))
            {
                return Ok(userId);
            }
            return Unauthorized(new { Message = "Geçersiz kullanıcı kimliği formatı." });
        }

        [HttpGet("roles")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserRoles()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Kullanıcı kimliği alınamadı." });
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new { Message = "Kullanıcı bulunamadı." });
            }

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            return Ok(roles);
        }

    }
}