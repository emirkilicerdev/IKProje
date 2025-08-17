using System;
using System.Collections.Generic;

namespace WebAPI.Dtos
{
    public class UserDetailDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new List<string>(); // Kullanıcının sahip olduğu rol adları
        public List<int> RoleIds { get; set; } = new List<int>(); // Kullanıcının sahip olduğu rol ID'leri

        public DateTime CreatedAt { get; set; }
    }
}
