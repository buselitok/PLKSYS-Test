using Microsoft.AspNetCore.Mvc;
using PLKSYS.Core.Interfaces; // IUserService için
using PLKSYS.Core.Models; // User, UserCreateDto, UserUpdateDto modelleri için
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // [Authorize] attribute'u için
using System.Security.Claims; // ClaimTypes için (kullanıcının rolünü ve ID'sini almak için)
using System;
using PLKSYS.Core.Data;
using Microsoft.EntityFrameworkCore;
using PLKSYS.Core.Helpers; // ArgumentException ve Exception için

namespace PLKSYS.API.Controllers
{
    [Route("api/[controller]")] // API yolu: /api/Users
    [ApiController] // Bu sınıfın bir API controller'ı olduğunu belirtir
    [Authorize(Policy = "AdminOrSuperadmin")] // Politika kullanıldı
    public class UsersController : ControllerBase
    {
        private readonly PLKSYSContext _context;

        public UsersController(PLKSYSContext context)
        {
            _context = context;
        }

        // GET: api/Users
        // Tüm kullanıcıları listeler
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users); // HTTP 200 OK ile kullanıcı listesini döndür
            }
            catch (Exception ex)
            {
                // Hata detaylarını konsola yazdır
                Console.WriteLine($"Kullanıcılar getirilirken API'de hata oluştu: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                // İstemciye daha açıklayıcı bir hata mesajı döndür
                return StatusCode(500, new { message = $"Kullanıcılar getirilirken sunucu tarafında bir hata oluştu: {ex.Message}" });
            }
        }

        // GET: api/Users/{id}
        // Belirli bir ID'ye sahip kullanıcıyı getirir
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // POST: api/Users
        // Yeni bir kullanıcı oluşturur
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] UserCreateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kullanıcı adı zaten mevcut mu kontrol et
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                return BadRequest(new { message = "Bu kullanıcı adı zaten mevcut." });
            }

            try
            {
                // Şifre hashleme
                var (passwordHash, passwordSalt) = AuthHelper.CreatePasswordHash(userDto.Password);

                var user = new User
                {
                    Username = userDto.Username,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Department = userDto.Department,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Kullanıcı oluşturulurken bir hata oluştu: {ex.Message}" });
            }
        }

        // PUT: api/Users/{id}
        // Mevcut bir kullanıcıyı günceller
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest(new { message = "ID uyuşmazlığı." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userToUpdate = await _context.Users.FindAsync(id);
            if (userToUpdate == null)
            {
                return NotFound(new { message = "Kullanıcı bulunamadı." });
            }

            // Kendi departmanını değiştirmeyi engelle (Admin ve Superadmin hariç)
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserDepartment = User.FindFirst(ClaimTypes.Role)?.Value; // Departman rol olarak kullanılıyor

            // Eğer giriş yapan kullanıcı kendi hesabını güncelliyorsa ve Admin veya Superadmin departmanında değilse
            if (currentUserIdClaim != null && int.Parse(currentUserIdClaim) == id && currentUserDepartment != "Admin" && currentUserDepartment != "Superadmin")
            {
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser != null && userDto.Department != existingUser.Department)
                {
                    return Forbid("Kendi departmanınızı değiştirmeye yetkiniz yok.");
                }
            }

            // Kullanıcı adı değişikliği varsa ve yeni kullanıcı adı zaten mevcutsa
            if (userToUpdate.Username != userDto.Username && await _context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                return BadRequest(new { message = "Bu kullanıcı adı zaten mevcut." });
            }

            try
            {
                userToUpdate.Username = userDto.Username;
                userToUpdate.FirstName = userDto.FirstName;
                userToUpdate.LastName = userDto.LastName;
                userToUpdate.Department = userDto.Department; // Departman güncellendi
                userToUpdate.UpdatedAt = DateTime.UtcNow;

                _context.Entry(userToUpdate).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Kullanıcı güncellenirken bir hata oluştu: {ex.Message}" });
            }
        }

        // DELETE: api/Users/{id}
        // Belirli bir ID'ye sahip kullanıcıyı siler
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Kendi hesabını silmeyi engelle
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserIdClaim != null && int.Parse(currentUserIdClaim) == id)
            {
                return Forbid("Kendi hesabınızı silemezsiniz.");
            }

            // Bir Admin veya Superadmin'in başka bir Admin veya Superadmin'i silmesini engelle
            var currentUserDepartment = User.FindFirst(ClaimTypes.Role)?.Value; // Departman rol olarak kullanılıyor
            if (currentUserDepartment == "Admin" || currentUserDepartment == "Superadmin")
            {
                var userToDelete = await _context.Users.FindAsync(id);
                if (userToDelete != null && (userToDelete.Department == "Admin" || userToDelete.Department == "Superadmin"))
                {
                    return Forbid("Bir Admin veya Superadmin, Admin veya Superadmin departmanındaki kullanıcıları silemez.");
                }
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Kullanıcı silinirken bir hata oluştu: {ex.Message}" });
            }
        }
    }
}