using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class QuestionCreateViewModel
    {
        public Question question { get; set; }
        public List<SelectListItem>? lesson { get; set; }
    }
}
