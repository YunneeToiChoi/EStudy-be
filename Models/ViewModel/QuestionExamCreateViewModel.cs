using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class QuestionExamCreateViewModel
    {
        public Question question { get; set; }
        public List<SelectListItem> exam { get; set; }
    }
}
