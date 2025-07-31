using PLKSYS.Core.Models;
using System.Threading.Tasks;

namespace PLKSYS.Core.Interfaces
{
    public interface IAuthService
    {
        Task<User?> Login(LoginRequest request);
        string CreateToken(User user);
    }
}