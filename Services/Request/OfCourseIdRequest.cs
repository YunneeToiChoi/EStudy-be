using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class OfCourseIdRequest
    {
        [Required]
        public required int courseId { get; set; }
    }
}
