using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Services;
using System.Linq;

namespace study4_be.Repositories
{
    public class UserRepository
    {
        private readonly Study4Context _context = new Study4Context();
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

        public async Task AddUser(User user)
        {
            // Tạo ID ngẫu nhiên cho người dùng
            user.UserId = Guid.NewGuid().ToString();
            HashPassword(user);
            _context.Users.Add(user);
            user.UserBanner = null;
            user.UserImage = "https://firebasestorage.googleapis.com/v0/b/estudy-426108.appspot.com/o/avtDefaultUser.jfif?alt=media&token=8dabba5f-cccb-4a4c-9ab4-69049c769bdf";
            user.Isverified = false;
            await _context.SaveChangesAsync();

        }
        public async Task AddUserWithServices(User user)
        {
            user.UserId = Guid.NewGuid().ToString(); // still gen id 
            _context.Users.Add(user);
            user.UserBanner = null;
            // add img to firebase

            user.UserImage = user.UserImage ?? "https://firebasestorage.googleapis.com/v0/b/estudy-426108.appspot.com/o/avtDefaultUser.jfif?alt=media&token=8dabba5f-cccb-4a4c-9ab4-69049c769bdf";
            user.Isverified = true; // is alway true if login with social services 
            await _context.SaveChangesAsync();
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
