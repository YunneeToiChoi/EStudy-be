using study4_be.Services.Course;
namespace study4_be.Interface.Course
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseResponse>> GetAllCoursesAsync();
        Task<IEnumerable<CourseResponse>> GetUnregisteredCoursesAsync(GetUserCoursesRequest request);
        Task<IEnumerable<CourseResponse>> GetOutstandingCoursesUserNotBoughtAsync(GetUserCoursesRequest request);
        Task<List<int>> GetCoursesUserHasPurchasedAsync(string userId);
        Task DeleteAllCoursesAsync();
    }
}
