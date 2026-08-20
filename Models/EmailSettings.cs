namespace UserManagementApp.Models
{
    public class EmailSettings
    {
        public string SenderEmail { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;

        public string ResendApiKey { get; set; } = string.Empty;
    }
}