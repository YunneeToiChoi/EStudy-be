using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class LessonEditViewModel
    {
        public Lesson lesson { get; set; }
        public List<SelectListItem>? container { get; set; }
        public List<SelectListItem>? tags { get; set; }
    }
}
