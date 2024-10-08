using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request.Lesson
{
    public class OfLessonRequest
    {
        [Required]
        public required int lessonId { get; set; }
    }
}
