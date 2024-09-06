using Microsoft.AspNetCore.Mvc;

namespace study4_be.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MarketingController : Controller
    {
        public IActionResult MarketingHome()
        {
            return View();
        }
    }
}
