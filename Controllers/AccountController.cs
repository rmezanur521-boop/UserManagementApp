using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementApp.Models;
using UserManagementApp.Services;
using UserManagementApp.ViewModels;

namespace UserManagementApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly BackgroundEmailQueue _emailQueue;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            BackgroundEmailQueue emailQueue,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailQueue = emailQueue;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Status = UserStatus.Unverified,
                RegisteredAt = DateTime.UtcNow
            };

            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(model);
                }
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                ModelState.AddModelError(string.Empty, "This email address is already registered.");
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var appUrl = _configuration["AppUrl"];
            var confirmLink = $"{appUrl}/Account/ConfirmEmail?userId={user.Id}&token={encodedToken}";
            var safeHref = System.Net.WebUtility.HtmlEncode(confirmLink);
            _emailQueue.Enqueue(new EmailJob
            {
                ToEmail = user.Email,
                Subject = "Verify your email",
                HtmlBody = $"<p>Hello {user.FullName},</p><p>Click <a href=\"{safeHref}\">here</a> to verify your email address.</p>"
            });

            TempData["StatusMessage"] = "Registration successful. Please check your email to verify your account.";

            return RedirectToAction("Index", "Users");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded && user.Status == UserStatus.Unverified)
            {
                user.Status = UserStatus.Active;
                await _userManager.UpdateAsync(user);
            }

            TempData["StatusMessage"] = result.Succeeded
                ? "Email verified successfully."
                : "Email verification failed or link expired.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            if (user.Status == UserStatus.Blocked)
            {
                ModelState.AddModelError(string.Empty, "This account has been blocked.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            user.LastLoginTime = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index", "Users");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException != null &&
                   ex.InnerException.Message.Contains("duplicate key value violates unique constraint");
        }
    }
} 