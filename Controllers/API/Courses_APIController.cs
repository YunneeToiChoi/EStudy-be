using Microsoft.AspNetCore.Mvc;
using study4_be.Interface.Course;
using study4_be.Models;
using study4_be.Services.Course;
using study4_be.Interface.Rating;
namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Courses_APIController : Controller
    {
        private readonly Study4Context _context;
        private readonly ICourseService _courseService;

        public Courses_APIController (Study4Context context, ICourseService courseService)
        {
            _context = context;
            _courseService = courseService;
        }

        [HttpGet("Get_AllCourses")]
        public async Task<ActionResult> Get_AllCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                return Ok(new { status = 200, message = "Get Courses Successful", courses });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 400, message = ex.Message });
            }
        }

        [HttpPost("Get_UnregisteredCourses")]
        public async Task<ActionResult> Get_UnregisteredCourses(GetUserCoursesRequest request)
        {
            try
            {
                var unregisteredCourses = await _courseService.GetUnregisteredCoursesAsync(request);
                return Ok(new { status = 200, message = "Get Unregistered Courses Successful", unregisteredCourses });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 400, message = ex.Message });
            }
        }

        [HttpPost("Get_OutstandingCoursesUserNotBought")]
        public async Task<ActionResult> Get_OutstandingCoursesUserNotBought(GetUserCoursesRequest request)
        {
            try
            {
                var outstandingCourses = await _courseService.GetOutstandingCoursesUserNotBoughtAsync(request);
                return Ok(new { status = 200, message = "Get Outstanding Courses Successful", outstandingCourses });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 400, message = ex.Message });
            }
        }

        [HttpDelete("Delete_AllCourses")]
        public async Task<ActionResult> Delete_AllCourses()
        {
            try
            {
                await _courseService.DeleteAllCoursesAsync();
                return Ok(new { status = 200, message = "Delete Courses Successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 400, message = ex.Message });
            }
        }
    }
}
