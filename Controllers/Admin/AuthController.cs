using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Models.ViewModel;

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
                var user = await _context.Staff.FindAsync(model.Email); 

                // Kiểm tra thông tin đăng nhập
                if (user != null && model.Email == user.StaffEmail && model.Password == user.StaffPassword) 
                {
                    // Đăng nhập thành công, bạn có thể thiết lập session hoặc cookie ở đây
                    // Chuyển hướng đến dashboard hoặc trang chính
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
            //await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth"); // Chuyển hướng về trang đăng nhập
        }
    }
}
