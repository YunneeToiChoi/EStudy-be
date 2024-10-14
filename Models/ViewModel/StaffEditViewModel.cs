using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class StaffEditViewModel
    {
        public Staff Staff { get; set; }
        public List<SelectListItem> Departments { get; set; }
    }
}
