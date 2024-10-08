using study4_be.Services.Lesson;

namespace study4_be.Services.Container
{
    public class ContainerResponse
    {
        public int ContainerId { get; set; }
        public string? ContainerTitle { get; set; }
        public List<LessonResponse> Lessons { get; set; }
    }
}
