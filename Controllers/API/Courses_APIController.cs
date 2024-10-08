using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Validation;
using NuGet.Protocol.Core.Types;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using study4_be.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using study4_be.Services.Course;
namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Courses_APIController : Controller
    {
        private readonly CourseRepository _coursesRepository;
        private readonly Study4Context _context;

        public Courses_APIController (Study4Context context)
        {
            _context = context;
            _coursesRepository = new(context);
        }

        [HttpGet("Get_AllCourses")] 
        public async Task<ActionResult<IEnumerable<Course>>> Get_AllCourses()
        {
            var courses = await _coursesRepository.GetAllCoursesAsync();
            var coursesResponse = courses.Select(course => new CourseResponse
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseDescription = course.CourseDescription,
                CourseImage = course.CourseImage,
                CoursePrice = course.CoursePrice,
                CourseSale = course.CourseSale,
                LastPrice = course.CoursePrice - ( course.CoursePrice * course.CourseSale / 100) ,
            }).ToList();
            return Json(new { status = 200, message = "Get Courses Successful", coursesResponse });
        }
        [HttpPost("Get_UnregisteredCourses")] // courses the user hasn't registered for
        public async Task<ActionResult> Get_UnregisteredCourses(GetUserCoursesRequest request)
        {
            List<int> registeredCourseIds = await GetCoursesUserHasPurchasedAsync(request.userId);

            var unregisteredCourses = await _context.Courses
                .Where(c => !registeredCourseIds.Contains(c.CourseId)) // Sử dụng CourseId từ bảng Courses
                .ToListAsync();

            if (unregisteredCourses == null || !unregisteredCourses.Any())
            {
                return NotFound(new { status = 404, message = "No unregistered courses found or User Register All Before" });
            }
            var unregisteredCoursesResponse = unregisteredCourses.Select(course => new CourseResponse
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseDescription = course.CourseDescription,
                CourseImage = course.CourseImage,
                CoursePrice = course.CoursePrice,
                CourseSale = course.CourseSale,
                LastPrice = course.CoursePrice - (course.CoursePrice * course.CourseSale / 100),
            }).ToList();
            return Ok(new { status = 200, message = "Get Unregistered Courses Successful", unregisteredCoursesResponse });
        }
        [HttpPost("Get_OutstandingCoursesUserNotBought")]
        public async Task<ActionResult> Get_OutstandingCoursesUserNotBought(GetUserCoursesRequest req)
        {
            if (req.userId == null || req.userId=="")
            {   
                var outstandingCoursesForGuest = await _context.UserCourses
                .GroupBy(uc => uc.CourseId)
                .Take(req.amountOutstanding)
                .Select(g => g.Key)// Chọn ra CourseId
                .ToListAsync();

                var detailedOutstandingCoursesForGuest = await _context.Courses
                      .Where(c => outstandingCoursesForGuest.Contains(c.CourseId)) // Lọc các khóa học theo danh sách các CourseId nổi bật
                      .ToListAsync();
                var detailedOutstandingCoursesForGuestResponse = detailedOutstandingCoursesForGuest.Select(course => new CourseResponse
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseDescription = course.CourseDescription,
                    CourseImage = course.CourseImage,
                    CoursePrice = course.CoursePrice,
                    CourseSale = course.CourseSale,
                    LastPrice = course.CoursePrice - (course.CoursePrice * course.CourseSale / 100),
                }).ToList();
                return Ok(new { status = 200, message = "Get Outstanding Courses For Guest Successful", outstandingCourses = detailedOutstandingCoursesForGuestResponse });
            }
            List<int>  userPurchasedCourses = await GetCoursesUserHasPurchasedAsync(req.userId);
            var outstandingCourses = await _context.UserCourses
                .Where(uc => !userPurchasedCourses.Contains(uc.CourseId)) // Lọc các khóa học chưa mua
                .GroupBy(uc => uc.CourseId) // Nhóm theo CourseId
                .OrderByDescending(g => g.Count()) // Sắp xếp giảm dần theo số lần xuất hiện
                .Select(g => g.Key)// Chọn ra CourseId
                .Take(req.amountOutstanding)
                .ToListAsync();

            // Lấy thông tin chi tiết của các khóa học nổi bật
            var detailedOutstandingCourses = await _context.Courses
                .Where(c => outstandingCourses.Contains(c.CourseId)) // Lọc các khóa học theo danh sách các CourseId nổi bật
                .ToListAsync();
                
            if (detailedOutstandingCourses == null || !detailedOutstandingCourses.Any())
                {
                    return NotFound(new { status = 404, message = "No outstanding courses found or not have any outstaind course" });
                }
            var detailedOutstandingCoursesResponse = detailedOutstandingCourses.Select(course => new CourseResponse
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseDescription = course.CourseDescription,
                CourseImage = course.CourseImage,
                CoursePrice = course.CoursePrice,
                CourseSale = course.CourseSale,
                LastPrice = course.CoursePrice - (course.CoursePrice * course.CourseSale / 100),
            }).ToList();
            return Ok(new { status = 200, message = "Get Outstanding Courses User Hadn't Bought Successful", outstandingCourses = detailedOutstandingCoursesResponse });
        }
        [HttpPost]
        public async Task<List<int>> GetCoursesUserHasPurchasedAsync(string userId)
        {
                var userPurchaseBought = await _context.UserCourses
                    .Where(uc => uc.UserId == userId)
                    .Select(uc => uc.CourseId)
                    .ToListAsync();
                return userPurchaseBought;
        }
        [HttpDelete("Delete_AllCourses")]
        public async Task<IActionResult> Delete_AllCourses()
        {
            await _coursesRepository.DeleteAllCoursesAsync();
            return Json(new { status = 200, message = "Delete Courses Successful" });
        }
    }
}
