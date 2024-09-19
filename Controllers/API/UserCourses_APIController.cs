using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;
using study4_be.Services.Request;
using System.Linq;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserCourses_APIController : Controller
    {
        private Study4Context _context = new Study4Context();
        private  UserCoursesRepository _userCoursesRepo = new UserCoursesRepository();
        [HttpPost("Get_AllCoursesByUser")]
        public async Task<ActionResult<IEnumerable<Course>>> Get_AllCoursesByUser(GetAllCoursesByUserRequest request)
        {
            var courseListId = await _userCoursesRepo.Get_AllCoursesByUser(request.userId);
            if (!courseListId.Any())
            {
                return Json(new { status = 404, message = "No courses found for the user" });
            }
            // Truy vấn để lấy danh sách khóa học dựa trên danh sách CourseId
            var courses = await _context.Courses
                .Where(c => courseListId.Contains(c.CourseId))  // So sánh trực tiếp với danh sách int
                .ToListAsync();
            return Json(new { status = 200, message = "Get All Courses By User Successful", courses });
        }

        [HttpPost("Get_DetailCourseAndUserBought")]
        public async Task<ActionResult<IEnumerable<User>>> Get_DetailCourseAndUserBought(GetAllUsersBuyCourse request)
        {
            var userList = await _userCoursesRepo.Get_DetailCourseAndUserBought(request.courseId);
            var totalAmount = userList.Count();
            var courseDetail = await _context.Courses.FindAsync(request.courseId);
            var finalPrice =  courseDetail.CoursePrice - (courseDetail.CoursePrice * courseDetail.CourseSale / 100);
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
