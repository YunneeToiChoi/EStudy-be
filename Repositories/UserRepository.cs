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
            return _context.Users.FirstOrDefault(u => u.UsersName == username);
        }
        public User GetUserByUserEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.UsersEmail == email);
        }

        public void AddUser(User user)
        {
            // Tạo ID ngẫu nhiên cho người dùng
            user.UsersId = Guid.NewGuid().ToString();
            HashPassword(user);
            _context.Users.Add(user);
            _context.SaveChanges();
        }
        public bool CheckEmailExists(string email)
        {
            return _context.Users.Any(u => u.UsersEmail == email);
        }
        public void HashPassword(User user)
        {
            user.UsersPassword = BCrypt.Net.BCrypt.HashPassword(user.UsersPassword);
        }
        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
    }
}
