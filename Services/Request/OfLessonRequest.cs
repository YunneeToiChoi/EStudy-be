using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class OfLessonRequest
    {
        [Required]
        public required int lessonId { get; set; }
    }
}
