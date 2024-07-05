using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class VideoEditViewModel
    {
        public Video video { get; set; }

        public List<SelectListItem> lesson { get; set; }
    }
}
