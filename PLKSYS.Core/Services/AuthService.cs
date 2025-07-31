using PLKSYS.Core.Data;
using PLKSYS.Core.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using PLKSYS.Core.Interfaces;
using PLKSYS.Core.Models; // IConfiguration için

namespace PLKSYS.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly PLKSYSContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(PLKSYSContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User?> Login(LoginRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return null; // Kullanıcı bulunamadı
            }

            // Şifre doğrulama
            if (!AuthHelper.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return null; // Yanlış şifre
            }

            // User modelinde Token özelliği olmadığı için burada atama yapılmaz.
            // Token LoginResponse ile döndürülür.
            return user;
        }

        public string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                // Departmanı doğrudan ClaimTypes.Role olarak kullanıyoruz
                new Claim(ClaimTypes.Role, user.Department),
                new Claim("Department", user.Department) // Departmanı özel claim olarak da ekle
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7), // Token 7 gün geçerli
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}