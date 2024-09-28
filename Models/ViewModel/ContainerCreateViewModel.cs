using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class ContainerCreateViewModel
    {
        public Container containers { get; set; }
        public List<SelectListItem>? listUnits {get; set;}
    }
}
