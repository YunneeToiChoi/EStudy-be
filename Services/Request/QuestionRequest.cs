using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class QuestionRequest
    {
        [Required]
        public required int lessonId { get; set; }   
    }
}
