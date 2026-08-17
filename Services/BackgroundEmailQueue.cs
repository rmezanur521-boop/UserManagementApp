using System.Threading.Channels;

namespace UserManagementApp.Services
{
    public class EmailJob
    {
        public string ToEmail { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string HtmlBody { get; set; } = string.Empty;
    }

    public class BackgroundEmailQueue
    {
        private readonly Channel<EmailJob> _channel = Channel.CreateUnbounded<EmailJob>();

        public void Enqueue(EmailJob job)
        {
            _channel.Writer.TryWrite(job);
        }

        public IAsyncEnumerable<EmailJob> DequeueAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}