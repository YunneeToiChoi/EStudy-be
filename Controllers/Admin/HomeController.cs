using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using study4_be.Models;

namespace study4_be.Controllers.Admin;
public class HomeController : Controller
{
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }
    [AllowAnonymous]
    [HttpGet("google-response")]
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
    public IActionResult Unauthorized()
    {
        return View();
    }
}
