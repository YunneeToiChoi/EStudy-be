using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class LessonCreateViewModel
    {
        public Lesson lesson { get; set; }
        public List<SelectListItem>? container { get; set; }   
        public List<SelectListItem>? tag { get; set; }
    }
}
