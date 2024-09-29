using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class VideoCreateViewModel
    {
        public Video videos { get; set; }
        public List<SelectListItem>? Lessons { get; set; }
    }
}
