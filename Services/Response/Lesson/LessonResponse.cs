namespace study4_be.Services.Response.Lesson
{
    public class LessonResponse
    {
        public int LessonId { get; set; }
        public string LessonType { get; set; } = string.Empty;
        public string LessonTitle { get; set; } = string.Empty;
        public string tagId { get; set; } = string.Empty;
    }
}
