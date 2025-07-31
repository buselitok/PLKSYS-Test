// PLKSYS.Core/Interfaces/IUserService.cs
using PLKSYS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PLKSYS.Core.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsers();
        Task<User?> GetUser(int id);
        Task<User?> AddUser(UserCreateDto userDto);
        Task UpdateUser(UserUpdateDto userDto);
        Task DeleteUser(int id);
        Task<User?> GetUserByIdAsync(int id); // YENİ EKLENEN METOT
    }
}
