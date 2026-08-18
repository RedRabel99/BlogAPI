using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure.Email;

namespace BlogAPI.IntegrationTests.Infrastructure;

public class TestEmailSender : IEmailSender
{
    public bool ShouldThrow { get; set; }

    private readonly List<EmailMessage> _sent = [];
    public IReadOnlyList<EmailMessage> Sent => _sent;


    public Task SendEmailAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (ShouldThrow) throw new InvalidOperationException("SMTP provider done (test purpose)");
        _sent.Add(message);
        return Task.CompletedTask;
    }

    public void Reset()
    {
        ShouldThrow = false;
        _sent.Clear();
    }
}
