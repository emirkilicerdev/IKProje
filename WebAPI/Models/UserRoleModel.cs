using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    [Table("UserRoles")]
    public class UserRoleModel
    {
        // Composite key olacak, Fluent API ile tanımlanmalı

        public int UserId { get; set; }
        public int RoleId { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserModel User { get; set; } = null!;

        [ForeignKey(nameof(RoleId))]
        public RoleModel Role { get; set; } = null!;
    }
}
