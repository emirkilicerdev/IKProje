namespace WebAPI.Dtos
{
    public class LoginResponseModel
    {
        public string Message { get; set; } = default!;
        public bool Success { get; set; }
        public string? Token { get; set; } // JWT token
        public List<string>? AvailableRoles { get; set; } // Kullanıcının sahip olduğu roller
    }
}
