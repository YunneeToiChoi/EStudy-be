using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Controllers.Admin;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services.User;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Unit_APIController : ControllerBase
    {
        private readonly UnitRepository _unitRepo;
        private readonly Study4Context _context;

        public Unit_APIController(Study4Context context) 
        { 
            _context = context;
            _unitRepo = new(context);
        }
        [HttpPost("Get_AllUnitsByCourse")]
        public async Task<ActionResult> Get_AllUnitsByCourse(GetAllUnitsByCourses courses)
        {
            // Check if the user is enrolled in the course
            var isUserEnrolledInCourse = await _context.UserCourses
                .AnyAsync(uc => uc.UserId == courses.userId && uc.CourseId == courses.courseId);

            if (!isUserEnrolledInCourse)
            {
                return BadRequest(new { status = 400, message = "User does not have this course" });
            }
            var units = await _unitRepo.GetAllUnitsByCourseAsync(courses.courseId);
            return Ok(new { status = 200, message = "Get All Units Successful", units });
        }
    }
}
