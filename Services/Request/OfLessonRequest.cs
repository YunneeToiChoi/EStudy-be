using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class OfLessonRequest
    {
        [Required]
        public int lessonId { get; set; }
    }
}
