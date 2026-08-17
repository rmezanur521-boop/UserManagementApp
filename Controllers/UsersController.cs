using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementApp.Data;
using UserManagementApp.Models;
using UserManagementApp.ViewModels;

namespace UserManagementApp.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.LastLoginTime)
                .Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    Status = u.Status.ToString(),
                    LastLoginTime = u.LastLoginTime
                })
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block([FromBody] string[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return Json(new { success = false, message = "No users selected." });
            }

            var users = await _context.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

            foreach (var user in users)
            {
                user.Status = UserStatus.Blocked;
            }

            await _context.SaveChangesAsync();

            var currentUserId = _userManager.GetUserId(User);
            bool selfAffected = ids.Contains(currentUserId);

            return Json(new { success = true, selfAffected, message = $"{users.Count} user(s) blocked." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unblock([FromBody] string[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return Json(new { success = false, message = "No users selected." });
            }

            var users = await _context.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

            foreach (var user in users)
            {
                user.Status = UserStatus.Active;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, selfAffected = false, message = $"{users.Count} user(s) unblocked." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] string[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return Json(new { success = false, message = "No users selected." });
            }

            var users = await _context.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

            var currentUserId = _userManager.GetUserId(User);
            bool selfAffected = ids.Contains(currentUserId);

            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();

            return Json(new { success = true, selfAffected, message = $"{users.Count} user(s) deleted." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnverified()
        {
            var users = await _context.Users
                .Where(u => u.Status == UserStatus.Unverified)
                .ToListAsync();

            var currentUserId = _userManager.GetUserId(User);
            bool selfAffected = users.Any(u => u.Id == currentUserId);

            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();

            return Json(new { success = true, selfAffected, message = $"{users.Count} unverified user(s) deleted." });
        }
    }
}