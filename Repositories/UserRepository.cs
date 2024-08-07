﻿using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using System.Linq;

namespace study4_be.Repositories
{
    public class UserRepository
    {
        private readonly STUDY4Context _context = new STUDY4Context();
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task DeleteAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();
        }
        public User GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.UserName == username);
        }
        public User GetUserByUserEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.UserEmail == email);
        }

        public void AddUser(User user)
        {
            // Tạo ID ngẫu nhiên cho người dùng
            user.UserId = Guid.NewGuid().ToString();
            HashPassword(user);
            _context.Users.Add(user);
            user.UserBanner = null;
            user.UserImage = "https://firebasestorage.googleapis.com/v0/b/estudy-426108.appspot.com/o/avtDefaultUser.jfif?alt=media&token=8dabba5f-cccb-4a4c-9ab4-69049c769bdf";
            user.Isverified = false;
            _context.SaveChanges();
        }
        public bool CheckEmailExists(string email)
        {
            return _context.Users.AsNoTracking().Any(u => u.UserEmail == email);
        }
        public void HashPassword(User user)
        {
            user.UserPassword = BCrypt.Net.BCrypt.HashPassword(user.UserPassword);
        }
        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
    }
}
