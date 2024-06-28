using Microsoft.AspNetCore.Mvc;

namespace study4_be.Controllers.Admin
{
    public class EmployeesController : Controller
    {
        public IActionResult Employees_List()
        {
            return View();
        }
    }
}
