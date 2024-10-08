using study4_be.Models;

namespace study4_be.Services.Response.Question
{
    public class VocabFlashCardResponse
    {
        public int vocabId { get; set; }
        public int lessonId { get; set; }
        public string vocabName { get; set; } = string.Empty;
        public int lessonName { get; set; }
        public IEnumerable<Models.Lesson> Vocabs { get; set; }
    }
}
