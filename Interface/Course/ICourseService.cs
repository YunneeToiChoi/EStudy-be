using study4_be.Models.DTO;
using study4_be.Services.Course;

namespace study4_be.Interface.Rating
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
    }
}
