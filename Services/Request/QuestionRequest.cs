using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class QuestionRequest
    {
        [Required]
        public int lessonId { get; set; }   
    }
}
