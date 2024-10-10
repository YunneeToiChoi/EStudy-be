using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Services;
using System.Linq;

namespace study4_be.Repositories
{
    public class UserRepository
    {
        private readonly Study4Context _context;
        private readonly IConfiguration _configuration;
        public UserRepository(Study4Context context,IConfiguration configuration) { _context = context; _configuration = configuration; }
       

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
        public async Task AddUserAsync(User user)
        {
            string avtUserDefault = _configuration["Firebase:AvatarDefaultUser"];
            // Tạo ID ngẫu nhiên cho người dùng
            user.UserId = Guid.NewGuid().ToString();
            HashPassword(user);
            await _context.Users.AddAsync(user);
            user.UserBanner = null;
            user.UserImage = avtUserDefault;
            user.Isverified = false;
            await _context.SaveChangesAsync();

        }
        public async Task AddUserWithServices(User user)
        {
            user.UserId = Guid.NewGuid().ToString(); // still gen id 
            _context.Users.Add(user);
            user.UserBanner = null;
            // add img to firebase
            string avtUserDefault = _configuration["Firebase:AvatarDefaultUser"];
            user.UserImage = user.UserImage ?? avtUserDefault;
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
