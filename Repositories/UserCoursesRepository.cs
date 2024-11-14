using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using System.Linq;
using System.Threading.Tasks;

namespace study4_be.Repositories
{
    public class UserCoursesRepository
    {
        private readonly Study4Context _context;

        public UserCoursesRepository(Study4Context context) { _context = context; }
        public async Task<IEnumerable<UserCourse>> GetAllUserCoursesAsync()
        {
            return await _context.UserCourses.ToListAsync();
        }
        public async Task<IEnumerable<int>> Get_AllCoursesByUser(string idUser)
        {
            // Lấy danh sách các CourseId từ bảng UserCourses (các khóa học đã thanh toán)
            var userCourses = _context.UserCourses
                                      .Where(uc => uc.UserId == idUser && uc.State == true)
                                      .Select(uc => uc.CourseId);

            return userCourses;
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
