using Microsoft.AspNetCore.Mvc;
using PLKSYS.Core.Interfaces;
using PLKSYS.Core.Models;
using System.Threading.Tasks;

namespace PLKSYS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _authService.Login(request);

            if (user == null)
            {
                return Unauthorized(new LoginResponse { IsSuccess = false, Message = "Kullanıcı adı veya şifre yanlış." });
            }

            // Token'ı oluştur ve LoginResponse nesnesine ekle
            var token = _authService.CreateToken(user);

            return Ok(new LoginResponse { IsSuccess = true, Token = token, Message = "Giriş başarılı!" });
        }
    }
}