using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class VocabFlashCardRequest
    {
        [Required]
        public required int lessonId { get; set; }
    }
}
