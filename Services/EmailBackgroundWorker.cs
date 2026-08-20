namespace UserManagementApp.Services
{
    public class EmailBackgroundWorker : BackgroundService
    {
        private readonly BackgroundEmailQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailBackgroundWorker> _logger;

        public EmailBackgroundWorker(BackgroundEmailQueue queue, IServiceScopeFactory scopeFactory, ILogger<EmailBackgroundWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var job in _queue.DequeueAllAsync(stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                try
                {
                    await sender.SendAsync(job.ToEmail, job.Subject, job.HtmlBody);
                    _logger.LogInformation("Email sent to {ToEmail}", job.ToEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email send failed for {ToEmail}", job.ToEmail);
                }
            }
        }
    }
}