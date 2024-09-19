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
        public async Task<IEnumerable<int>> Get_AllCoursesByUser(string idUser)
        {
            // Lấy danh sách các CourseId từ bảng UserCourses (các khóa học đã thanh toán)
            var userCourses = _context.UserCourses
                                      .Where(uc => uc.UserId == idUser)
                                      .Select(uc => uc.CourseId);

            // Lấy danh sách các CourseId từ bảng Plan_Courses (các khóa học trong gói mà người dùng đã đăng ký)
            var planCourses = from pc in _context.PlanCourses
                              join us in _context.UserSubs on pc.PlanId equals us.PlanId
                              where us.UserId == idUser && us.State == true
                              select pc.CourseId;

            // Kết hợp cả hai danh sách và loại bỏ các CourseId trùng lặp
            var allCourses = await userCourses
                                    .Union(planCourses)    // Kết hợp danh sách từ cả hai bảng
                                    .Distinct()            // Loại bỏ khóa học trùng lặp
                                    .ToListAsync();

            return allCourses;
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
