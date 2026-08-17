namespace UserManagementApp.Services
{
    public class EmailBackgroundWorker : BackgroundService
    {
        private readonly BackgroundEmailQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;

        public EmailBackgroundWorker(BackgroundEmailQueue queue, IServiceScopeFactory scopeFactory)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
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
                }
                catch
                {
                }
            }
        }
    }
}