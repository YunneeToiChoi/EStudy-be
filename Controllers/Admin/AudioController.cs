using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;

namespace study4_be.Controllers.Admin
{
    public class AudioController : Controller
    {
        public IActionResult Audio_List()
        {
            return View();
        }
        public IActionResult Audio_Create()
        {
            return View();
        }
        public IActionResult Audio_Delete()
        {
            return View();
        }
        public IActionResult Audio_Edit()
        {
            return View();
        }
        public IActionResult Audio_Details()
        {
            return View();
        }
    }
}
