using Microsoft.AspNetCore.Identity;

namespace UserManagementApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public UserStatus Status { get; set; } = UserStatus.Unverified;

        public DateTime? LastLoginTime { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}