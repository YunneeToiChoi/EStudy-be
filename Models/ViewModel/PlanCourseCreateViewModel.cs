using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class PlanCourseCreateViewModel
    {
        public PlanCourse planCourse { get; set; }
        public int? oldPlanid { get; set; }
        public int? oldCourseid { get; set; }
        public List<SelectListItem>? plan { get; set; }
        public List<SelectListItem>? course { get; set; }
    }
}
