using System;
using System.ComponentModel.DataAnnotations;

namespace PLKSYS.Client.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Kullanıcı oluşturma için DTO
    public class UserCreateDto
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Ad zorunludur.")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Soyad zorunludur.")]
        public string LastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Departman zorunludur.")]
        public string Department { get; set; } = string.Empty;
        [Required(ErrorMessage = "Rol zorunludur.")]
        public string Role { get; set; } = string.Empty;
    }

    // Kullanıcı güncelleme için DTO
    public class UserUpdateDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string Username { get; set; } = string.Empty;
        // Şifre güncelleme için ayrı bir alan veya ayrı bir endpoint kullanılabilir
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}