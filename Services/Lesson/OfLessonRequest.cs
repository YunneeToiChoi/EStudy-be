using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Lesson
{
    public class OfLessonRequest
    {
        [Required]
        public required int lessonId { get; set; }
    }
}
