using Microsoft.AspNetCore.Mvc;

namespace study4_be.Controllers.Admin
{
    public class DepartmentController : Controller
    {
        public IActionResult Department_List()
        {
            return View();
        }
    }
}
