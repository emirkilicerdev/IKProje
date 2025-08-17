namespace WebAPI.Models
{
    public class AuthResponseData
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; } // Token'ın geçerlilik süresi
        public string Username { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } // Kullanıcının sahip olduğu tüm roller
    }

    // WebAPI/Dtos/AuthResponseModel.cs (veya uygun DTO dosyanız)
    public class AuthResponseModel
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public AuthResponseData Data { get; set; }
    }
}
