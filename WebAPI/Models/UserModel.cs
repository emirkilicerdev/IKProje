using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    [Table("Users")]
    public class UserModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = default!;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!;

        // Çoktan çoka ilişki
        public ICollection<UserRoleModel> UserRoles { get; set; } = new List<UserRoleModel>();

        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
