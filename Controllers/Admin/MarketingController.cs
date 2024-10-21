using Microsoft.AspNetCore.Mvc;

namespace study4_be.Controllers.Admin
{
    public class MarketingController : Controller
    {
        [Route("Finance/[controller]/[action]")]
        public IActionResult MarketingHome()
        {
            return View();
        }
    }
}
