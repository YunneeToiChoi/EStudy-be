using Microsoft.AspNetCore.Mvc;

namespace study4_be.Controllers.Admin
{
    public class HRController : Controller
    {
        public IActionResult HRHome()
        {
            return View();
        }
    }
}
