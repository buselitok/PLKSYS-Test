namespace PLKSYS.Core.Models
{
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public User? User { get; set; } // Giriş yapan kullanıcının bilgileri
    }
}