using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using study4_be.Models;

namespace study4_be.Controllers.Admin;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult SignInGoogle()
    {
        return View(); // Trả về view SignInGoogle.cshtml
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
