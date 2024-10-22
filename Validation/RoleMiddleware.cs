using Microsoft.AspNetCore.Http;
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
            if (!context.User.Identity.IsAuthenticated)
            {
                Console.WriteLine("User is not authenticated.");
                if (!context.Request.Path.StartsWithSegments("/Auth/Login"))
                {
                    context.Response.Redirect("/Auth/Login");
                    return;
                }
            }
            // Check if the user is authenticated
            else
            {
                Console.WriteLine("User is authenticated.");
                // Retrieve the user's role (assuming role is stored in claims or session)
                var userRole = context.User.FindFirst("Role")?.Value ?? context.Session.GetString("UserRole");

                // Get the current request path
                var path = context.Request.Path;

                // Role-based access rules
                if (path.StartsWithSegments("/HR") && userRole != "HR")
                {
                    // Redirect to an unauthorized page if the user is not an Admin
                    context.Response.Redirect("/Home/Unauthorized");
                    return;
                }
                else if (path.StartsWithSegments("/Finance") && userRole != "Finance")
                {
                    // Redirect to an unauthorized page if the user is not a User
                    context.Response.Redirect("/Home/Unauthorized");
                    return;
                }
                else if (path.StartsWithSegments("/CourseManager") && userRole != "CourseManager")
                {
                    // Redirect to an unauthorized page if the user is not a User
                    context.Response.Redirect("/Home/Unauthorized");
                    return;
                }
            }

            // Continue to the next middleware in the pipeline
            await _next(context);
        }
    }

}
