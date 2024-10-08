using Google.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services.Request.Course;
using study4_be.Services.Request.User;
using System.Linq;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserCourses_APIController : Controller
    {
        private readonly Study4Context _context;
        private readonly UserCoursesRepository _userCoursesRepo;

        public UserCourses_APIController(Study4Context context)
        {
            _context = context;
            _userCoursesRepo = new(context);
        }

        [HttpPost("Get_AllCoursesByUser")]
        public async Task<ActionResult<IEnumerable<Course>>> Get_AllCoursesByUser(GetAllCoursesByUserRequest request)
        {
            var courseListId = await _userCoursesRepo.Get_AllCoursesByUser(request.userId);
            var planCourseList = await (from pc in _context.PlanCourses
                                        join us in _context.UserSubs
                                        on pc.PlanId equals us.PlanId
                                        join sp in _context.Subscriptionplans
                                        on us.PlanId equals sp.PlanId
                                        where us.UserId == request.userId
                                        select new
                                        {
                                            pc.CourseId,
                                            sp.PlanName
                                        }).ToListAsync();

            if (!courseListId.Any())
            {
                return Json(new { status = 404, message = "No courses found for the user" });
            }
            // Truy vấn để lấy danh sách khóa học dựa trên danh sách CourseId
            var courses = await _context.Courses
                .Where(c => courseListId.Contains(c.CourseId))  // So sánh trực tiếp với danh sách int
                .ToListAsync();
            return Json(new { status = 200, message = "Get All Courses By User Successful", courses , planCourseId = planCourseList});
        }

        [HttpPost("Get_DetailCourseAndUserBought")]
        public async Task<ActionResult<IEnumerable<User>>> Get_DetailCourseAndUserBought(GetAllUsersBuyCourse request)
        {
            // Lấy danh sách người dùng đã mua riêng course này
            var userList = await _userCoursesRepo.Get_DetailCourseAndUserBought(request.courseId);
            // Lấy danh sách tất cả các plan mà course này thuộc về
            var plansContainingCourse = await _context.PlanCourses
                .Where(pc => pc.CourseId == request.courseId)
                .Select(pc => pc.PlanId)
                .ToListAsync();
            // Lấy danh sách người dùng đã đăng ký các plan chứa course này
            var usersWithPlanForCourse = await _context.UserSubs
                .Where(us => plansContainingCourse.Contains(us.PlanId))
                .Select(us => us.UserId)
                .Distinct() // Đảm bảo không tính trùng người dùng đã đăng ký nhiều plan
                .ToListAsync();
            // Gộp danh sách người dùng mua riêng course với người dùng đăng ký plan chứa course
            var totalAmount = userList.Count() + usersWithPlanForCourse.Count;
            // Lấy chi tiết khóa học
            var courseDetail = await _context.Courses.FindAsync(request.courseId);
            // Tính giá cuối cùng của khóa học
            var finalPrice = courseDetail.CoursePrice - (courseDetail.CoursePrice * courseDetail.CourseSale / 100);
            return Json(new { status = 200, message = "Get Detail course and user Bought course", courseDetail,finalPrice, totalAmount, userList });
        }
        [HttpGet("Get_AllUserCourses")]
        public async Task<ActionResult<IEnumerable<User>>> Get_AllUserCourses()
        {
            var courses = await _userCoursesRepo.GetAllUserCoursesAsync();
            return Json(new { status = 200, message = "Get All UserCourses Successful", courses });
        }
        [HttpDelete("Delete_AllUserCourses")]
        public async Task<ActionResult<IEnumerable<User>>> Delete_AllUserCourses()
        {
            try
            {
                await _userCoursesRepo.Delete_AllUsersCoursesAsync();
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return BadRequest(ex.Message);
            }
            return Json(new { status = 200, message = "Delete All UserCourses Successful"});
        }
    }
}
