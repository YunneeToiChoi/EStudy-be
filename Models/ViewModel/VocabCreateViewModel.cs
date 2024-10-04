using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class VocabCreateViewModel
    {
        public Vocabulary vocab {  get; set; }  
        public List<SelectListItem>? lesson { get; set; }
    }
}
