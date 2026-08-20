using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UserManagementApp.Models;

namespace UserManagementApp.Services
{
    public class ResendEmailSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly EmailSettings _settings;

        public ResendEmailSender(HttpClient httpClient, IOptions<EmailSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var payload = new
            {
                from = $"{_settings.SenderName} <{_settings.SenderEmail}>",
                to = new[] { toEmail },
                subject = subject,
                html = htmlBody
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ResendApiKey);

            var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);
            response.EnsureSuccessStatusCode();
        }
    }
}