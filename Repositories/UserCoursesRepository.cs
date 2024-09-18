using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using System.Linq;
using System.Threading.Tasks;

namespace study4_be.Repositories
{
    public class UserCoursesRepository
    {
        private readonly Study4Context _context = new Study4Context();
        public async Task<IEnumerable<UserCourse>> GetAllUserCoursesAsync()
        {
            return await _context.UserCourses.ToListAsync();
        }
        public async Task DeleteAllUserCoursesAsync()
        {
            var userCourses = await _context.UserCourses.ToListAsync();
            _context.UserCourses.RemoveRange(userCourses);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<UserCourse>> Get_AllCoursesByUser(string idUser)
        {
            return await _context.UserCourses
                                  .Where(uc => uc.UserId == idUser)
                                  .ToListAsync();
        }
        public async Task<IEnumerable<UserCourse>> Get_DetailCourseAndUserBought(int idCourse)
        {
            return await _context.UserCourses
                                  .Where(uc => uc.CourseId == idCourse)
                                  .ToListAsync();
        }
        public async Task Delete_AllUsersCoursesAsync()
        {
            var userCourses = await _context.UserCourses.ToListAsync();
            _context.UserCourses.RemoveRange(userCourses);
            await _context.SaveChangesAsync();
        }
    }
}
