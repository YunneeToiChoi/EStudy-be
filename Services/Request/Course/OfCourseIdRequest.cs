using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request.Course
{
    public class OfCourseIdRequest
    {
        [Required]
        public required int courseId { get; set; }
    }
}
