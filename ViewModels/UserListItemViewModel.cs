namespace UserManagementApp.ViewModels
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime? LastLoginTime { get; set; }
    }
}