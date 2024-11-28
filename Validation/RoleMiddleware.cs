using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
namespace study4_be.Validation
{
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Lấy đường dẫn hiện tại
            var path = context.Request.Path;

            // Kiểm tra nếu người dùng đã xác thực và yêu cầu đến từ Admin
            if (path.StartsWithSegments("/Admin"))
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    // Nếu người dùng chưa xác thực, chuyển hướng đến trang đăng nhập
                    context.Response.Redirect("/Auth/Login");
                    return;
                }

                // Lấy vai trò của người dùng từ claims hoặc session
                var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value ?? context.Session.GetString("UserRole");

                // Kiểm tra quyền truy cập dựa trên vai trò
                if (path.StartsWithSegments("/Admin/HR") && userRole != "HR")
                {
                    // Chuyển hướng đến trang không có quyền truy cập nếu người dùng không phải là HR
                    context.Response.Redirect("/Home/Unauthorized");
                    return;
                }
                else if (path.StartsWithSegments("/Admin/Finance") && userRole != "Finance")
                {
                    // Chuyển hướng đến trang không có quyền truy cập nếu người dùng không phải là Finance
                    context.Response.Redirect("/Home/Unauthorized");
                    return;
                }
                else if (path.StartsWithSegments("/Admin/CourseManager") && userRole != "CourseManager")
                {
                    // Chuyển hướng đến trang không có quyền truy cập nếu người dùng không phải là CourseManager
                    context.Response.Redirect("/Home/Unauthorized");
                    return;
                }
            }

            // Tiếp tục xử lý yêu cầu trong pipeline
            await _next(context);
        }
    }

}
