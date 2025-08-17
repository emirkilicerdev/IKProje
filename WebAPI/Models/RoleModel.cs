using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    [Table("Roles")]
    public class RoleModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = default!;

        // Çoktan çoka ilişki
        public ICollection<UserRoleModel> UserRoles { get; set; } = new List<UserRoleModel>();
    }
}
