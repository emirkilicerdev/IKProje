using System.Collections.Generic; // List için
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class UserUpdateDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Kullanıcı adı en az 3, en fazla 100 karakter olmalıdır.")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [StringLength(255, ErrorMessage = "E-posta adresi en fazla 255 karakter olmalıdır.")]
        public string? Email { get; set; }

        public string? Password { get; set; } // Şifre güncellemesi için opsiyonel

        // YENİ: Tek RoleId yerine RoleId'lerin opsiyonel dizisi
        public List<int>? RoleIds { get; set; }
    }
}
