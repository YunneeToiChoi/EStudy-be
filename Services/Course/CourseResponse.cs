using study4_be.Models;

namespace study4_be.Services.Course
{
    public class CourseResponse
    {
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseDescription { get; set; }
        public string? CourseImage { get; set; }
        public string? CourseTag { get; set; }
        public double? CoursePrice { get; set; }
        public int? CourseSale { get; set; }
        public double? LastPrice { get; set; }
        public List<Models.Course> FreeCourses { get; set; }
        public List<Models.Course> PaidCourses { get; set; }
    }
}
