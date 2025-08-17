using System.Collections.Generic; // List için
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class UserCreateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Kullanıcı adı en az 3, en fazla 100 karakter olmalıdır.")]
        public string Username { get; set; } = default!;

        [Required]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [StringLength(255, ErrorMessage = "E-posta adresi en fazla 255 karakter olmalıdır.")]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6, en fazla 100 karakter olmalıdır.")]
        public string Password { get; set; } = default!;

        // YENİ: Tek RoleId yerine RoleId'lerin dizisi
        [Required(ErrorMessage = "En az bir rol ID'si zorunludur.")]
        public List<int> RoleIds { get; set; } = new List<int>();
    }
}
