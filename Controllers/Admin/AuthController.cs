using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using System.Security.Claims;

namespace study4_be.Controllers.Admin
{
    public class AuthController : Controller
    {
        private readonly Study4Context _context;
        public AuthController (Study4Context context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model) // Sửa: Thêm Task vào IActionResult
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Staff.FirstOrDefaultAsync(s => s.StaffEmail == model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User is not exist.");
                    return View();
                }
               
                // Kiểm tra thông tin đăng nhập
                if (user != null && model.Email == user.StaffEmail && model.Password == user.StaffPassword) 
                {
                    // Đăng nhập thành công, bạn có thể thiết lập session hoặc cookie ở đây
                    // Chuyển hướng đến dashboard hoặc trang chính
                    var userRole = await _context.Roles.FindAsync(user.RoleId);

                    HttpContext.Session.SetString("UserRole", userRole.RoleName);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.StaffEmail),
                        new Claim(ClaimTypes.Role, userRole.RoleName)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }

                // Nếu thông tin đăng nhập không hợp lệ
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Sign the user out (this clears the authentication cookie)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Auth"); // Chuyển hướng về trang đăng nhập
        }
    }
}
