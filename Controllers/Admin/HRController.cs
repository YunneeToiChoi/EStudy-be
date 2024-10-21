using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{
    [Route("HR/[controller]/[action]")]
    public class HRController : Controller
    {
        public IActionResult HRHome()
        {
            return View();
        }

    }
}
