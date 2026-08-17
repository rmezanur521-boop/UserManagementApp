using Microsoft.AspNetCore.Identity;
using UserManagementApp.Models;

namespace UserManagementApp.Middleware
{
    public class UserStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public UserStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            var excludedPaths = new[]
            {
                "/account/login",
                "/account/register",
                "/lib",
                "/css",
                "/js"
            };

            bool isExcluded = excludedPaths.Any(p => path.StartsWith(p));

            if (!isExcluded && context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user == null || user.Status == UserStatus.Blocked)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}