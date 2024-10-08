using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Question
{
    public class VocabFlashCardRequest
    {
        [Required]
        public required int lessonId { get; set; }
    }
}
