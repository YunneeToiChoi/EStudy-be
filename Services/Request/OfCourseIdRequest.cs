using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class OfCourseIdRequest
    {
        [Required]
        public int courseId { get; set; }
    }
}
