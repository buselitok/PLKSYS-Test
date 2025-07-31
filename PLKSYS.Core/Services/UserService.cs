// PLKSYS.Core/Services/UserService.cs
// Kullanıcı yönetimi işlemleri için servis implementasyonu.
using PLKSYS.Core.Data;
using PLKSYS.Core.Helpers;
using PLKSYS.Core.Interfaces;
using PLKSYS.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PLKSYS.Core.Services
{
    public class UserService : IUserService
    {
        private readonly PLKSYSContext _context;

        public UserService(PLKSYSContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUser(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> AddUser(UserCreateDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                throw new ArgumentException("Bu kullanıcı adı zaten mevcut.");
            }

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
            return user;
        }

        public async Task UpdateUser(UserUpdateDto userDto)
        {
            var userToUpdate = await _context.Users.FindAsync(userDto.Id);
            if (userToUpdate == null)
            {
                throw new ArgumentException("Kullanıcı bulunamadı.");
            }

            if (userToUpdate.Username != userDto.Username && await _context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                throw new ArgumentException("Bu kullanıcı adı zaten mevcut.");
            }

            userToUpdate.Username = userDto.Username;
            userToUpdate.FirstName = userDto.FirstName;
            userToUpdate.LastName = userDto.LastName;
            userToUpdate.Department = userDto.Department;
            userToUpdate.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(userToUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new ArgumentException("Kullanıcı bulunamadı.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id) // YENİ METOT İMPLEMENTASYONU
        {
            return await _context.Users.FindAsync(id);
        }
    }
}
