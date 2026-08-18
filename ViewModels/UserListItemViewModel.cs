namespace UserManagementApp.ViewModels
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime? LastLoginTime { get; set; }

        public string LastSeenDisplay => LastLoginTime.HasValue
            ? GetRelativeTime(LastLoginTime.Value)
            : "N/A";

        public string? LastSeenExact => LastLoginTime?.ToString("MMMM dd, yyyy HH:mm:ss");

        private static string GetRelativeTime(DateTime pastTime)
        {
            var span = DateTime.UtcNow - pastTime;

            if (span.TotalSeconds < 60)
            {
                return "less than a minute ago";
            }

            if (span.TotalMinutes < 60)
            {
                var minutes = (int)span.TotalMinutes;
                return minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
            }

            if (span.TotalHours < 24)
            {
                var hours = (int)span.TotalHours;
                return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
            }

            if (span.TotalDays < 7)
            {
                var days = (int)span.TotalDays;
                return days == 1 ? "1 day ago" : $"{days} days ago";
            }

            var weeks = (int)(span.TotalDays / 7);
            return weeks == 1 ? "1 week ago" : $"{weeks} weeks ago";
        }
    }
}