using System.ComponentModel.DataAnnotations;

namespace WebAPI.Dtos
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string Username { get; set; } = default!;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Password { get; set; } = default!;
    }
}
