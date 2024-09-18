using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Services;

namespace study4_be.Repositories
{
    public class CourseRepository
    {
        private readonly Study4Context _context = new Study4Context();
        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.ToListAsync();
        }
        public async Task DeleteAllCoursesAsync()
        {
            var courses = await _context.Courses.ToListAsync();
            _context.Courses.RemoveRange(courses);
            await _context.SaveChangesAsync();
        }
    }
}
