using Microsoft.AspNetCore.Mvc;

namespace study4_be.Controllers.Admin
{
    public class MarketingController : Controller
    {
        public IActionResult MarketingHome()
        {
            return View();
        }
    }
}
