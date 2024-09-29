using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class VocabFlashCardRequest
    {
        [Required]
        public int lessonId { get; set; }
    }
}
