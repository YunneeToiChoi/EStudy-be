
using Google;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using study4_be.Interface.Rating;
using study4_be.Models;
using study4_be.Models.DTO;
using study4_be.Services.Request.Course;
using study4_be.Services.Request.Document;
using study4_be.Services.Request.Rating;
using study4_be.Services.Request.User;
using study4_be.Services.Response;
using study4_be.Services.Response.Course;

namespace study4_be.Services.Rating
{
    public class CourseService : ICourseService
    {
        private readonly Study4Context _context;
        public CourseService(Study4Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            var courses = await _context.Courses.ToListAsync();
            return courses.Select(c => new CourseDto
            {
                courseId = c.CourseId,
                courseName = c.CourseName
            });
        }
   
    }
}
